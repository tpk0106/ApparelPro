using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SRN1.PRG (data entry/save) and IN_SRN2.PRG (print) —
    // "SUPPLIER RETURN NOTE". Returns a quantity of an item that is currently held under a
    // Buyer/Order's physical stock back to a Supplier, decrementing QtyInHand exactly the
    // way a GIN (Goods Issue Note) does, but against a Supplier instead of a Department.
    //
    // Architectural deviation from legacy, called out per SKILL.md's Mandatory Architectural
    // Justifications rule: like GTN/GRN/RTN/STRN, this service commits a whole note as one
    // atomic DB transaction with UPDLOCK/HOLDLOCK row locks taken per line, rather than
    // replicating IN_SRN1.PRG's live ShadowBalance ("shdw_bal") reservation mutation on every
    // line typed into the temp grid before the note is confirmed.
    //
    // Pre-Migration Defect Audit finding (per SKILL.md's Pre-Migration Code Defect &
    // Performance Audit rule): IN_SRN1.PRG's over-quantity guard silently discards the
    // offending line and continues the entry loop rather than raising a hard error — an
    // operator could easily miss that a return was dropped. This service instead throws
    // InvalidOperationException and aborts the whole transaction on the first invalid line,
    // matching the stricter (and now-established) precedent already applied to GTN's
    // "Attempt to Exceed Balance Quantity" guard.
    public class SupplierReturnNoteService : ISupplierReturnNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public SupplierReturnNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<SrnReturnableStockRowServiceModel>> GetReturnableStockByBuyerOrderAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "Invalid Buyer/Order.  [F1] - Help"
            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            // Mirrors legacy's "copy whil buyer+order = xbuyer+xorder .and. qty_in_hd > 0 to temp1"
            // snapshot — only rows that actually have something to return.
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.QtyInHand > 0)
                .ToListAsync();

            if (stockRows.Count == 0)
                return new List<SrnReturnableStockRowServiceModel>();

            // Description resolution mirrors GoodsTransferNoteService/GoodsReturnNoteService's
            // established two-tier convention: StyleMaterialCostProfiles (od_sacc2) is the
            // authoritative, style/feature-specific source, keyed by the full 22-char composite
            // ItemCode; StockItems is a plain ItemCode -> Description catalog keyed by just the
            // 4-char base item code (StockCode(2) stripped off), used only as a fallback.
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

                return new SrnReturnableStockRowServiceModel
                {
                    ItemCode = s.ItemCode,
                    StoreCode = s.StoreCode,
                    Unit = s.Unit,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    QtyInHand = s.QtyInHand,
                    MaxReturnableQuantity = s.QtyInHand,
                    // Same formula as StoresRequisitionService's own Avail check — advisory
                    // only, the hard ceiling above is still just QtyInHand.
                    NetAvailableAfterOutstandingRequisitions = s.QtyInHand - s.ShadowBalance - s.StrnBalance,
                };
            }).ToList();
        }

        public async Task<bool> CommitSupplierReturnNoteAsync(
            SrnHeaderServiceModel header,
            List<SrnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Supplier Return Note cannot be committed without line items.");

            header.Order = header.Order.Trim();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate the Buyer/Order — never trust a client echo, even though
                    // the frontend will have already called GetReturnableStockByBuyerOrderAsync.
                    var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
                    if (!buyerOrderExists)
                        throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

                    // 2. Validate the Supplier — mirrors legacy "seek m_supp_cd" on od_supp,
                    // "Invalid Supplier Code."
                    var supplierExists = await _apparelProDbContext.Suppliers
                        .AsNoTracking()
                        .AnyAsync(s => s.SupplierCode == header.SupplierCode);
                    if (!supplierExists)
                        throw new InvalidOperationException($"Invalid Supplier Code '{header.SupplierCode}'.");

                    // 3. Thread-safe allocation of the next SRN number.
                    string allocatedSrnNumber = await _sharedService.GenerateNextDocumentNumberAsync("SRN");
                    header.SrnNumber = allocatedSrnNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        if (line.Quantity <= 0)
                            throw new InvalidOperationException($"Quantity must be greater than zero for Item '{line.ItemCode}'.");

                        // 4. Validate Basis code — mirrors legacy "seek m_store_cd" on od_bref,
                        // "Invalid Basis Code."
                        var basisExists = await _apparelProDbContext.Basis
                            .AsNoTracking()
                            .AnyAsync(b => b.Code == line.StoreCode);
                        if (!basisExists)
                            throw new InvalidOperationException($"Invalid Basis Code '{line.StoreCode}'.");

                        // 5. Validate Unit code — mirrors legacy "seek m_unit" on od_uref,
                        // "Invalid Unit Code."
                        var unitExists = await _apparelProDbContext.Units
                            .AsNoTracking()
                            .AnyAsync(u => u.Code == line.Unit);
                        if (!unitExists)
                            throw new InvalidOperationException($"Invalid Unit Code '{line.Unit}'.");

                        // 6. Lock and validate the physical stock row — mirrors legacy
                        // "seek xbuyer+xorder+m_store_cd+m_item_cd" on in_stock, "Item Code not
                        // found." Same UPDLOCK/HOLDLOCK reasoning as
                        // GoodsTransferNoteService/GoodsIssueService: another concurrent
                        // GIN/GRN/RTN/GTN could be mutating this exact row's QtyInHand right now.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in stock.");

                        decimal requestedInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.Quantity);

                        // 7. Hard block — mirrors legacy "m_qty > qty_in_hd", "Attempt to
                        // Exceed Balance Quantity." Unlike IN_SRN1.PRG (which silently drops the
                        // line and continues), this aborts the whole commit — see the
                        // Pre-Migration Defect Audit note at the top of this class.
                        if (requestedInStockUnit > stockRecord.QtyInHand)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.QtyInHand)} {line.Unit}.");

                        // 8. Write the SRN transaction row. "7S" is the exact legacy code
                        // (IN_SRN1.PRG: "repl ... id with '7S'") — kept as-is rather than
                        // inventing a new one, same rigor already applied to GTN's "6T"/"1T".
                        var srnRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedSrnNumber,
                            TransactionType = "7S",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.Quantity,
                            SupplierCode = header.SupplierCode,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(srnRow);

                        // 9. Update the physical stock ledger — mirrors legacy's
                        // "qty_in_hd with qty_in_hd-mx_qty".
                        stockRecord.QtyInHand -= requestedInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 10. Update the order-level master running total — mirrors legacy's
                        // "sret_qty with sret_qty+mx_qty". Soft-updated only if the master row
                        // exists, same "if found()" precedent already established in
                        // GoodsIssueService/GoodsReturnNoteService/GoodsTransferNoteService.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.SupplierReturnQuantity += requestedInMasterUnit;
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
