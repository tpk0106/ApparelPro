using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using ApparelPro.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // "Stock Movement (for an Item)" report - a chronological transaction ledger for a
    // single Buyer/Order/Item, with a running balance column. This is a different report
    // shape from the sibling StockMovementReportService (which shows per-item running
    // TOTALS across an entire order) - migrated from legacy IN_SMVE1.PRG, whereas the
    // sibling report is migrated from IN_SMVE2.PRG.
    public class StockMovementItemReportService : IStockMovementItemReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public StockMovementItemReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private enum BalanceEffect { Add, Subtract, Set, None }

        // ---------------------------------------------------------------------------
        // Transaction type -> (display name, balance effect), replicated exactly from
        // legacy IN_SMVE1.PRG's "do case" block. All codes except "0X" are confirmed
        // live against the current BusinessLogic services (grepped
        // TransactionType = "..." across apparelPro.BusinessLogic/Services/Implementation/
        // OrderwiseInventory/*.cs) - "0X" (Additional Receipts Note) has no modern module
        // writing it yet, so it will simply never appear in real data today; it's kept
        // here so the report is future-proof and matches legacy 1:1.
        //
        // IMPORTANT - confirmed discrepancy between the two legacy Stock Movement reports,
        // both intentionally preserved as-is per your explicit instruction ("how does the
        // legacy system calculate? do as it is") rather than reconciled:
        //   - IN_SMVE1.PRG (THIS report, for an Item): "4X" Additional Issue Note has ZERO
        //     effect on the running balance. The line `*m_bal = m_bal - convert(...)` is
        //     commented out in the legacy source and replaced with `m_bal = m_bal + 0`.
        //   - IN_SMVE2.PRG (the sibling Order-level StockMovementReportService/
        //     StockMovementReportEngine): DOES subtract AdditionalIssuedQuantity from the
        //     balance.
        // Do not "fix" this report to match the other one - they are different legacy
        // programs with different (legacy-confirmed) behavior.
        //
        // "3A" Stock Adjustment Note is also special: legacy SETS the balance to the
        // transaction quantity (`m_bal = convert(unit,m_unit,qty)`) rather than adding or
        // subtracting it - modeled below as BalanceEffect.Set.
        // ---------------------------------------------------------------------------
        private static readonly IReadOnlyDictionary<string, (string Name, BalanceEffect Effect)> TransactionEffects =
            new Dictionary<string, (string, BalanceEffect)>
            {
                ["GR"] = ("Goods Received Note", BalanceEffect.Add),
                ["0X"] = ("Additional Receipts Note", BalanceEffect.Add),
                ["0S"] = ("Stores Requisition Note", BalanceEffect.None),
                ["1T"] = ("Goods Transfer Note (In)", BalanceEffect.Add),
                ["2R"] = ("Goods Return Note", BalanceEffect.Add),
                ["3A"] = ("Stock Adjustment Note", BalanceEffect.Set),
                ["4I"] = ("Goods Issue Note", BalanceEffect.Subtract),
                ["4X"] = ("Additional Issue Note", BalanceEffect.None), // confirmed: no balance effect (see note above)
                ["5D"] = ("Damaged Goods Note", BalanceEffect.Subtract),
                ["6T"] = ("Goods Transfer Note (Out)", BalanceEffect.Subtract),
                ["7S"] = ("Supplier Return Note", BalanceEffect.Subtract),
            };

        public async Task<List<StockMovementItemOptionServiceModel>> GetAvailableItemsAsync(int buyerCode, string order)
        {
            order = order.Trim();

            return await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .GroupBy(s => new { s.ItemCode, s.Description })
                .Select(g => new StockMovementItemOptionServiceModel
                {
                    ItemCode = g.Key.ItemCode,
                    Description = g.Key.Description,
                })
                .OrderBy(i => i.ItemCode)
                .ToListAsync();
        }

        // Balance is inherently sequential/stateful, so the full transaction history is
        // always pulled and replayed chronologically in memory first - there is no way to
        // compute a correct running balance for page 2+ using SQL-level Skip/Take alone.
        // OrderwiseStockTransaction.TransactionDate is a SQL `date` column (no time
        // component persisted), so same-day transactions are ordered by Id (insertion
        // order) as the tiebreaker, standing in for legacy's date+time+id+docno index.
        private async Task<List<StockMovementItemReportLineServiceModel>> BuildLedgerAsync(int buyerCode, string order, string itemCode)
        {
            var transactions = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.BuyerCode == buyerCode && t.Order == order && t.ItemCode == itemCode)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.Id)
                .ToListAsync();

            var lines = new List<StockMovementItemReportLineServiceModel>(transactions.Count);
            decimal balance = 0m;

            foreach (var transaction in transactions)
            {
                var (name, effect) = TransactionEffects.TryGetValue(transaction.TransactionType, out var mapped)
                    ? mapped
                    : (transaction.TransactionType, BalanceEffect.None);

                balance = effect switch
                {
                    BalanceEffect.Add => balance + transaction.Quantity,
                    BalanceEffect.Subtract => balance - transaction.Quantity,
                    BalanceEffect.Set => transaction.Quantity,
                    _ => balance,
                };

                lines.Add(new StockMovementItemReportLineServiceModel
                {
                    TransactionDate = transaction.TransactionDate,
                    DocumentNumber = transaction.DocumentNumber,
                    TransactionType = transaction.TransactionType,
                    TransactionTypeName = name,
                    Unit = transaction.Unit,
                    Quantity = transaction.Quantity,
                    BalanceAfter = balance,
                });
            }

            return lines;
        }

        public async Task<StockMovementItemReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order, string itemCode)
        {
            order = order.Trim();
            itemCode = itemCode.Trim();

            // Mirrors legacy: "seek xbuyer+xorder" on od_po, "do error with 'Invalid Buyer/Order'"
            var purchaseOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!purchaseOrderExists)
                throw new KeyNotFoundException($"Invalid Buyer/Order (Buyer: {buyerCode}, Order: {order}).");

            var styleProfile = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.ItemCode == itemCode)
                .Select(s => new { s.Description, s.ItemUnit, s.TotalConsumption })
                .FirstOrDefaultAsync();

            // Mirrors legacy: "seek" against the restricted item lookup, "do error with 'Invalid Item Code'"
            if (styleProfile is null)
                throw new KeyNotFoundException($"Invalid Item Code (Buyer: {buyerCode}, Order: {order}, Item: {itemCode}).");

            var lines = await BuildLedgerAsync(buyerCode, order, itemCode);

            // Mirrors legacy: "do error with 'No transactions found for this item'"
            if (lines.Count == 0)
                throw new InvalidOperationException($"No transactions for item code (Buyer: {buyerCode}, Order: {order}, Item: {itemCode}).");

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            return new StockMovementItemReportHeaderServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : buyerCode.ToString(),
                Order = order,
                ItemCode = itemCode,
                Description = styleProfile.Description,
                Unit = styleProfile.ItemUnit,
                OrderQuantity = styleProfile.TotalConsumption,
                TransactionCount = lines.Count,
                ClosingBalance = lines[^1].BalanceAfter,
            };
        }

        public async Task<PaginationResult<StockMovementItemReportLineServiceModel>> GetLinesAsync(
            int buyerCode, string order, string itemCode, int pageSize, int currentPage)
        {
            order = order.Trim();
            itemCode = itemCode.Trim();

            var allLines = await BuildLedgerAsync(buyerCode, order, itemCode);
            var pageItems = allLines.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            return new PaginationResult<StockMovementItemReportLineServiceModel>(
                pageSize, currentPage, allLines.Count, pageItems, null, null, null, null);
        }

        public async Task<List<StockMovementItemReportLineServiceModel>> GetLinesForPdfAsync(int buyerCode, string order, string itemCode)
        {
            order = order.Trim();
            itemCode = itemCode.Trim();
            return await BuildLedgerAsync(buyerCode, order, itemCode);
        }
    }
}
