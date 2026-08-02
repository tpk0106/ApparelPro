using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_DGN1.PRG (data entry/save) and IN_DGN2.PRG (print) —
    // "DAMAGED GOODS NOTE". Writes off a quantity of an item that is currently held under
    // a Buyer/Order's physical stock as damaged, decrementing QtyInHand and incrementing
    // OrderwiseStock.DamagedQuantity in the same physical stock row, in the same
    // transaction. Unlike SRN/GTN, there is no Supplier or destination Order involved —
    // this is a pure in-place write-off against the Buyer/Order it already belongs to.
    //
    // Architectural deviation from legacy, called out per SKILL.md's Mandatory Architectural
    // Justifications rule: like GTN/GRN/RTN/STRN/SRN, this service commits a whole note as
    // one atomic DB transaction with UPDLOCK/HOLDLOCK row locks taken per line, rather than
    // replicating IN_DGN1.PRG's line-by-line temp grid mutation before the note is confirmed.
    //
    // Pre-Migration Defect Audit finding (per SKILL.md's Pre-Migration Code Defect &
    // Performance Audit rule): same class of defect already found in SRN/GTN — IN_DGN1.PRG's
    // over-quantity guard silently discards the offending line and continues the entry loop
    // rather than raising a hard error. This service instead throws
    // InvalidOperationException and aborts the whole transaction on the first invalid line.
    public class DamagedGoodsNoteService : IDamagedGoodsNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public DamagedGoodsNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<DgnDamageableStockRowServiceModel>> GetDamageableStockByBuyerOrderAsync(
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
            // snapshot — only rows that actually have something to write off.
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.QtyInHand > 0)
                .ToListAsync();

            if (stockRows.Count == 0)
                return new List<DgnDamageableStockRowServiceModel>();

            // Description resolution mirrors the established two-tier convention used by
            // every other note type: StyleMaterialCostProfiles (od_sacc2) is the
            // authoritative, style/feature-specific source, keyed by the full 22-char
            // composite ItemCode; StockItems is a plain ItemCode -> Description catalog
            // keyed by just the 4-char base item code (StockCode(2) stripped off), used
            // only as a fallback.
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

                return new DgnDamageableStockRowServiceModel
                {
                    ItemCode = s.ItemCode,
                    StoreCode = s.StoreCode,
                    Unit = s.Unit,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    QtyInHand = s.QtyInHand,
                    MaxDamageableQuantity = s.QtyInHand,
                };
            }).ToList();
        }

        public async Task<bool> CommitDamagedGoodsNoteAsync(
            DgnHeaderServiceModel header,
            List<DgnLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Damaged Goods Note cannot be committed without line items.");

            header.Order = header.Order.Trim();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate the Buyer/Order — never trust a client echo, even though
                    // the frontend will have already called GetDamageableStockByBuyerOrderAsync.
                    var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
                    if (!buyerOrderExists)
                        throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

                    // 2. Thread-safe allocation of the next DGN number. No Supplier
                    // validation step here — legacy IN_DGN1.PRG's header is just
                    // Buyer/Order/Date, unlike SRN which also carries a Supplier.
                    string allocatedDgnNumber = await _sharedService.GenerateNextDocumentNumberAsync("DGN");
                    header.DgnNumber = allocatedDgnNumber;

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
                        // found." Same UPDLOCK/HOLDLOCK reasoning as every other note type:
                        // another concurrent GIN/GRN/RTN/GTN/SRN could be mutating this exact
                        // row's QtyInHand right now.
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

                        // 6. Hard block — mirrors legacy "m_qty > qty_in_hd", "Attempt to
                        // Exceed Balance Quantity." Unlike IN_DGN1.PRG (which silently drops
                        // the line and continues), this aborts the whole commit — see the
                        // Pre-Migration Defect Audit note at the top of this class.
                        if (requestedInStockUnit > stockRecord.QtyInHand)
                            throw new InvalidOperationException($"Attempt to Exceed Balance Quantity for Item '{line.ItemCode}'. Requested: {line.Quantity} {line.Unit}, Available: {await _sharedService.ConvertUnitAsync(stockRecord.Unit, line.Unit, stockRecord.QtyInHand)} {line.Unit}.");

                        // 7. Write the DGN transaction row. "5D" is the exact legacy code
                        // (IN_DGN1.PRG: "id with '5D'") — kept as-is rather than inventing a
                        // new one, same rigor already applied to every other note type.
                        var dgnRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedDgnNumber,
                            TransactionType = "5D",
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
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(dgnRow);

                        // 8. Update the physical stock ledger — mirrors legacy's
                        // "qty_in_hd with qty_in_hd-mx_qty, damg_qty with damg_qty+mx_qty".
                        // Both fields on the same OrderwiseStock row move together: the
                        // quantity leaves "in hand" and lands in "damaged" — it is not
                        // physically removed from the store, just reclassified.
                        stockRecord.QtyInHand -= requestedInStockUnit;
                        stockRecord.DamagedQuantity += requestedInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 9. Update the order-level master running total — mirrors legacy's
                        // "damg_qty with damg_qty+mx_qty" on in_stmst. Soft-updated only if
                        // the master row exists, same "if found()" precedent already
                        // established in every other note type's service.
                        var masterRow = await _apparelProDbContext.OrderwiseStockMasters
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStockMasters WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (masterRow != null)
                        {
                            decimal requestedInMasterUnit = await _sharedService.ConvertUnitAsync(line.Unit, masterRow.Unit, line.Quantity);
                            masterRow.DamagedQuantity += requestedInMasterUnit;
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
