using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_RTN1.PRG (data entry/save) — "GOODS RETURN NOTE".
    // Reverses part of a previous Goods Issue: floor/department returns unused or
    // excess material for a Buyer/Order back into store stock. Structurally this is
    // the inverse of GoodsIssueService: instead of decrementing QtyInHand and
    // checking against it, a return increments QtyInHand and checks against
    // ToDateIssued (you cannot return more than was ever issued).
    public class GoodsReturnNoteService : IGoodsReturnNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public GoodsReturnNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<RtnReturnableStockRowServiceModel>> GetReturnableStockByBuyerOrderAsync(int buyerCode, string order)
        {
            order = order.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "Invalid Buyer/Order.  [F1] - Help"
            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            // Mirrors legacy's "copy whil buyer+order = xbuyer+xorder to temp1" snapshot,
            // narrowed to rows that actually have something to return.
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.ToDateIssued > 0)
                .ToListAsync();

            if (stockRows.Count == 0)
                return new List<RtnReturnableStockRowServiceModel>();

            // Description resolution mirrors StoresRequisitionService's STRN print lookup
            // exactly (bug fix — the previous version here queried StockItems by the full
            // 22-char composite ItemCode, which never matches, so Description was always
            // "(No description available)"):
            // 1) StyleMaterialCostProfiles is the authoritative, style/feature-specific
            //    source, keyed by the full 22-char composite ItemCode.
            // 2) StockItems is a plain ItemCode -> Description catalog keyed by just the
            //    4-char base item code (StockCode(2) stripped off), used only as a fallback.
            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles.ToDictionary(p => p.ItemCode.Trim(), p => p);

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = stockRows.Select(s => DecomposePart(s.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            return stockRows.Select(s =>
            {
                var baseItemCode = DecomposePart(s.ItemCode, 2, 4);
                profileByItemCode.TryGetValue(s.ItemCode.Trim(), out var matchingProfile);

                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(baseItemCode, out description);

                return new RtnReturnableStockRowServiceModel
                {
                    ItemCode = s.ItemCode,
                    StoreCode = s.StoreCode,
                    Unit = s.Unit,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    QtyInHand = s.QtyInHand,
                    MaxReturnableQuantity = s.ToDateIssued,
                };
            }).ToList();
        }

        public async Task<bool> CommitGoodsReturnNoteAsync(
            RtnHeaderServiceModel header,
            List<RtnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Goods Return Note cannot be committed without line items.");

            header.Order = header.Order.Trim();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate Buyer/Order — never trust a client echo, even though the
                    // frontend will have already called GetReturnableStockByBuyerOrderAsync.
                    var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
                    if (!buyerOrderExists)
                        throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

                    // 2. Thread-safe allocation of the next RTN number. Legacy generates this
                    // lazily via autosave() on first confirmed line instead of up front — this
                    // codebase's established convention (GIN/GRN/STRN) allocates once at the
                    // start of the atomic commit instead, which is simpler and equally safe
                    // since the whole commit either fully succeeds or fully rolls back.
                    string allocatedRtnNumber = await _sharedService.GenerateNextDocumentNumberAsync("RTN");
                    header.RtnNumber = allocatedRtnNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                        // 3. Validate Basis code — mirrors legacy "seek m_store_cd" on od_bref,
                        // "Invalid Basis Code."
                        var basisExists = await _apparelProDbContext.Basis
                            .AsNoTracking()
                            .AnyAsync(b => b.Code == line.StoreCode);
                        if (!basisExists)
                            throw new InvalidOperationException($"Invalid Basis Code '{line.StoreCode}'.");

                        // 4. Validate Unit code — mirrors legacy "seek m_unit" on od_uref,
                        // "Invalid Unit Code."
                        var unitExists = await _apparelProDbContext.Units
                            .AsNoTracking()
                            .AnyAsync(u => u.Code == line.Unit);
                        if (!unitExists)
                            throw new InvalidOperationException($"Invalid Unit Code '{line.Unit}'.");

                        // 5. Lock and validate the physical stock row — mirrors legacy
                        // "seek xbuyer+xorder+m_store_cd+m_item_cd" on in_stock, "Item Code not
                        // found in Stock Master File." Same UPDLOCK/HOLDLOCK reasoning as
                        // GoodsIssueService/StoresRequisitionService: this row's QtyInHand and
                        // ToDateIssued are running totals another concurrent RTN/GIN could be
                        // mutating at the same time.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in the stock master file.");

                        decimal requestedInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);

                        // 6. Hard block — mirrors legacy "m_qty > convert(unit,m_unit,to_dt_iss)",
                        // "Return Quantity cannot be greater than Total issues."
                        if (requestedInStockUnit > stockRecord.ToDateIssued)
                            throw new InvalidOperationException($"Return Quantity cannot be greater than Total Issued for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Issued to date: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.ToDateIssued)} {line.Unit}.");

                        // 7. Write the RTN transaction row. TransactionType "2R" is the exact
                        // legacy code (IN_RTN1.PRG: "seek xdocno+'2R'") — kept as-is rather than
                        // inventing a new one, same rigor already applied to STRN's "0S".
                        // DepartmentCode has no legacy equivalent here (IN_RTN1.PRG never asks
                        // for one — only Buyer/Order/Store/Item/Unit/Qty), so it's left empty
                        // rather than repurposing an unrelated field.
                        var rtnRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedRtnNumber,
                            TransactionType = "2R",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(rtnRow);

                        // 8. Update physical stock ledger — mirrors legacy's
                        // "qty_in_hd with qty_in_hd+convert(...), to_dt_iss with to_dt_iss-convert(...)".
                        stockRecord.QtyInHand += requestedInStockUnit;
                        stockRecord.ToDateIssued -= requestedInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 9. Update order-level master running total. Legacy also decrements
                        // issd_qty and increments bal_qty directly here — this modernization
                        // keeps IssuedQuantity as a monotonic gross-issued audit total instead
                        // (see OrderwiseStockMaster.ReturnedQuantity comment) and only adds to
                        // the new ReturnedQuantity column; any derived balance calculation
                        // elsewhere (e.g. Stock Movement Report) needs to add ReturnedQuantity
                        // back in, which it does not do yet — flagged separately below.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.ReturnedQuantity += requestedInMasterUnit;
                            _apparelProDbContext.OrderwiseStockMasters.Update(masterRow);
                        }
                    }

                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
