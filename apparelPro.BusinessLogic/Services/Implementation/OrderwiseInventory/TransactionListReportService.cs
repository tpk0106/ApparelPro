using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_DLIST.PRG - "LIST OF TRANSACTIONS" (Orderwise). A
    // flat OrderwiseStockTransactions log for a date range, optional
    // type/item-code-prefix filter - no running balance (that's the Stock Movement
    // reports' job). Type-code -> display-name mapping reused verbatim from the
    // authoritative list already established in StockMovementItemReportService.
    public class TransactionListReportService : ITransactionListReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        private static readonly IReadOnlyDictionary<string, string> TransactionTypeNames = new Dictionary<string, string>
        {
            ["GR"] = "Goods Received Note",
            ["0X"] = "Additional Receipts Note",
            ["0S"] = "Stores Requisition Note",
            ["1T"] = "Goods Transfer Note (In)",
            ["2R"] = "Goods Return Note",
            ["3A"] = "Stock Adjustment Note",
            ["4I"] = "Goods Issue Note",
            ["4X"] = "Additional Issue Note",
            ["5D"] = "Damaged Goods Note",
            ["6T"] = "Goods Transfer Note (Out)",
            ["7S"] = "Supplier Return Note",
        };

        public TransactionListReportService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private async Task<(TransactionListReportHeaderServiceModel Header, List<TransactionListReportLineServiceModel> Lines)> BuildAsync(
            DateOnly fromDate, DateOnly toDate, string? transactionType, string? itemCodePrefix)
        {
            if (fromDate > toDate)
                throw new InvalidOperationException("[To Date] should be greater than [From Date]");

            transactionType = string.IsNullOrWhiteSpace(transactionType) ? null : transactionType.Trim().ToUpper();
            itemCodePrefix = string.IsNullOrWhiteSpace(itemCodePrefix) ? null : itemCodePrefix.Trim();

            var query = _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => t.TransactionDate >= fromDate.ToDateTime(TimeOnly.MinValue) && t.TransactionDate <= toDate.ToDateTime(TimeOnly.MaxValue));

            if (transactionType != null)
                query = query.Where(t => t.TransactionType == transactionType);
            if (itemCodePrefix != null)
                query = query.Where(t => t.ItemCode.StartsWith(itemCodePrefix));

            var transactions = await query
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.Id)
                .ToListAsync();

            var buyerOrderPairs = transactions.Select(t => (t.BuyerCode, Order: t.Order.Trim())).Distinct().ToList();
            var costProfiles = new List<ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile>();
            foreach (var (buyerCode, order) in buyerOrderPairs)
            {
                var rows = await _apparelProDbContext.StyleMaterialCostProfiles
                    .AsNoTracking()
                    .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                    .ToListAsync();
                costProfiles.AddRange(rows);
            }
            var profileByBuyerOrderItem = costProfiles
                .GroupBy(p => (p.BuyerCode, Order: p.Order.Trim(), ItemCode: p.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = transactions.Select(t => DecomposePart(t.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var supplierCodes = transactions.Where(t => t.SupplierCode.HasValue).Select(t => t.SupplierCode!.Value).Distinct().ToList();
            var supplierNames = await _apparelProDbContext.Suppliers
                .AsNoTracking()
                .Where(s => supplierCodes.Contains(s.SupplierCode))
                .ToDictionaryAsync(s => s.SupplierCode, s => s.Name);

            var buyerCodesInLines = transactions.Select(t => t.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodesInLines.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var lines = transactions.Select(t =>
            {
                var itemCode = t.ItemCode.Trim();
                var order = t.Order.Trim();
                profileByBuyerOrderItem.TryGetValue((t.BuyerCode, order, itemCode), out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                decimal price = t.Price ?? 0;

                return new TransactionListReportLineServiceModel
                {
                    TransactionType = t.TransactionType,
                    TransactionTypeName = TransactionTypeNames.GetValueOrDefault(t.TransactionType, t.TransactionType),
                    TransactionDate = DateOnly.FromDateTime(t.TransactionDate),
                    DocumentNumber = t.DocumentNumber,
                    StoreCode = t.StoreCode,
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Quantity = t.Quantity,
                    Unit = t.Unit,
                    Price = price,
                    Value = t.Quantity * price,
                    Currency = t.Currency ?? "",
                    SupplierName = t.SupplierCode.HasValue ? supplierNames.GetValueOrDefault(t.SupplierCode.Value, "") : "",
                    BuyerCode = t.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(t.BuyerCode, ""),
                    Order = order,
                };
            }).ToList();

            decimal totalValue = 0;
            string? totalCurrency = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.Currency))?.Currency;
            if (totalCurrency != null)
            {
                foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l.Currency)))
                    totalValue += await _currencyConversionService.ConvertAsync(line.Value, line.Currency, totalCurrency);
            }

            var header = new TransactionListReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TransactionType = transactionType,
                TransactionTypeName = transactionType != null ? TransactionTypeNames.GetValueOrDefault(transactionType, transactionType) : "ALL",
                ItemCodePrefix = itemCodePrefix,
                TotalLineItems = lines.Count,
                TotalValue = totalValue,
                TotalValueCurrency = totalCurrency,
            };

            return (header, lines);
        }

        public async Task<TransactionListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? transactionType, string? itemCodePrefix)
        {
            var (header, _) = await BuildAsync(fromDate, toDate, transactionType, itemCodePrefix);
            return header;
        }

        public async Task<List<TransactionListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? transactionType, string? itemCodePrefix)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate, transactionType, itemCodePrefix);
            return lines;
        }
    }
}
