using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SAN1.PRG (data entry/save) and IN_SAN2.PRG (print) —
    // "STOCK ADJUSTMENT NOTE". Unlike every other Orderwise Inventory note (GIN, GRN, RTN,
    // GTN, SRN, DGN — all movements that add to or subtract from QtyInHand), SAN is a
    // stock-take correction: the operator enters the true physical count for an item, and
    // this service SETS OrderwiseStock.QtyInHand to that value directly. There is no
    // ceiling to validate against — legacy IN_SAN1.PRG never compares the new count to the
    // old one, it just overwrites.
    //
    // Architectural deviation from legacy, called out per SKILL.md's Mandatory Architectural
    // Justifications rule: commits the whole note as one atomic DB transaction with
    // UPDLOCK/HOLDLOCK row locks per line, same as every other note type, rather than
    // replicating IN_SAN1.PRG's live temp-grid mutation before the note is confirmed.
    //
    // Pre-Migration Defect Audit finding (per SKILL.md's Pre-Migration Code Defect &
    // Performance Audit rule): legacy requires the entered quantity to be strictly > 0
    // ("valid m_qty > 0"), meaning a physical count of zero could never be recorded via
    // SAN — an accidental restriction rather than an intentional one. This service allows
    // AdjustedQuantity >= 0 instead, per explicit product decision, so a stock take can
    // correctly confirm an item is fully depleted.
    //
    // Note: unlike every other note type, this service does NOT touch OrderwiseStockMaster
    // at all. Legacy's in_stmst.adjs_qty/bal_qty updates map to LastAdjustmentQuantity
    // (already computed live from OrderwiseStockTransactions by StockMovementReportService)
    // and Balance (always derived, never persisted) — both already handled with zero
    // changes needed here.
    public class StockAdjustmentNoteService : IStockAdjustmentNoteService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public StockAdjustmentNoteService(
            ApparelProDbContext apparelProDbContext,
            ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        public async Task<List<SanAdjustableStockRowServiceModel>> GetAdjustableStockByBuyerOrderAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "Invalid Buyer/Order.  [F1] - Help"
            var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!buyerOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            // Mirrors legacy's "copy whil buyer+order = xbuyer+xorder to temp1" — no
            // qty_in_hd filter, unlike SRN/DGN. Every stock row is adjustable.
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();

            if (stockRows.Count == 0)
                return new List<SanAdjustableStockRowServiceModel>();

            // Description resolution mirrors the established two-tier convention used by
            // every other note type.
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

                return new SanAdjustableStockRowServiceModel
                {
                    ItemCode = s.ItemCode,
                    StoreCode = s.StoreCode,
                    Unit = s.Unit,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    QtyInHand = s.QtyInHand,
                };
            }).ToList();
        }

        public async Task<bool> CommitStockAdjustmentNoteAsync(
            SanHeaderServiceModel header,
            List<SanLineItemServiceModel> lines,
            string username)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException("Transaction Aborted: Stock Adjustment Note cannot be committed without line items.");

            header.Order = header.Order.Trim();

            using (var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Re-validate the Buyer/Order — never trust a client echo.
                    var buyerOrderExists = await _apparelProDbContext.PurchaseOrders
                        .AsNoTracking()
                        .AnyAsync(po => po.BuyerCode == header.BuyerCode && po.Order == header.Order);
                    if (!buyerOrderExists)
                        throw new InvalidOperationException($"Invalid Buyer/Order (Buyer: {header.BuyerCode}, Order: {header.Order}).");

                    // 2. Thread-safe allocation of the next SAN number.
                    string allocatedSanNumber = await _sharedService.GenerateNextDocumentNumberAsync("SAN");
                    header.SanNumber = allocatedSanNumber;

                    foreach (var line in lines)
                    {
                        line.StoreCode = line.StoreCode.Trim().ToUpper();
                        line.ItemCode = line.ItemCode.Trim();
                        line.Unit = line.Unit.Trim().ToUpper();

                        // Product decision: allow >= 0 rather than legacy's stricter > 0,
                        // so a stock take can record a genuinely depleted item.
                        if (line.AdjustedQuantity < 0)
                            throw new InvalidOperationException($"Adjusted Quantity cannot be negative for Item '{line.ItemCode}'.");

                        // 3. Validate Basis code.
                        var basisExists = await _apparelProDbContext.Basis
                            .AsNoTracking()
                            .AnyAsync(b => b.Code == line.StoreCode);
                        if (!basisExists)
                            throw new InvalidOperationException($"Invalid Basis Code '{line.StoreCode}'.");

                        // 4. Validate Unit code.
                        var unitExists = await _apparelProDbContext.Units
                            .AsNoTracking()
                            .AnyAsync(u => u.Code == line.Unit);
                        if (!unitExists)
                            throw new InvalidOperationException($"Invalid Unit Code '{line.Unit}'.");

                        // 5. Lock and validate the physical stock row — mirrors legacy
                        // "seek xbuyer+xorder+m_store_cd+m_item_cd" on in_stock, "Item not
                        // found in Stock Master File." Same UPDLOCK/HOLDLOCK reasoning as
                        // every other note type.
                        var stockRecord = await _apparelProDbContext.OrderwiseStocks
                            .FromSqlInterpolated($@"SELECT * FROM OrderwiseStocks WITH (UPDLOCK, HOLDLOCK)
                                WHERE BuyerCode = {header.BuyerCode}
                                  AND [Order] = {header.Order}
                                  AND StoreCode = {line.StoreCode}
                                  AND ItemCode = {line.ItemCode}")
                            .FirstOrDefaultAsync();

                        if (stockRecord == null)
                            throw new InvalidOperationException($"Item '{line.ItemCode}' under basis '{line.StoreCode}' does not exist in stock.");

                        decimal newQtyInStockUnit = await _sharedService.ConvertUnitAsync(line.Unit, stockRecord.Unit, line.AdjustedQuantity);

                        // 6. Write the SAN transaction row. "3A" is the exact legacy code
                        // (IN_SAN1.PRG: "id with '3A'") — corrected here rather than reusing
                        // the placeholder "AJ" that StockMovementReportService.cs guessed
                        // before this module existed.
                        var sanRow = new OrderwiseStockTransaction
                        {
                            DocumentNumber = allocatedSanNumber,
                            TransactionType = "3A",
                            TransactionDate = header.TransactionDate,
                            BuyerCode = header.BuyerCode,
                            Order = header.Order,
                            DepartmentCode = string.Empty,
                            StockCode = line.ItemCode.Substring(0, 2),
                            StoreCode = line.StoreCode,
                            ItemCode = line.ItemCode,
                            Unit = line.Unit,
                            Quantity = line.AdjustedQuantity,
                            CreatedByUsername = username.Trim().ToUpper()
                        };
                        await _apparelProDbContext.OrderwiseStockTransactions.AddAsync(sanRow);

                        // 7. SET (not add/subtract) the physical stock ledger — mirrors
                        // legacy's "repl qty_in_hd with mx_qty". This is the one place in
                        // the whole Orderwise Inventory module where QtyInHand is directly
                        // overwritten rather than incremented/decremented by a delta.
                        stockRecord.QtyInHand = newQtyInStockUnit;
                        _apparelProDbContext.OrderwiseStocks.Update(stockRecord);

                        // 8. No OrderwiseStockMaster update — see the class-level comment.
                        // LastAdjustmentQuantity is computed live from this transaction row
                        // by StockMovementReportService; Balance is always derived.
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
