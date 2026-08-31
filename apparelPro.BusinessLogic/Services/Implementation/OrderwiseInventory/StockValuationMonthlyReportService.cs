using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL1.PRG - "STOCK VALUATION REPORT (Monthly)".
    // Unlike StockValuationReportService (scoped to one Buyer/Order), this walks EVERY
    // OrderwiseStockTransaction of type GR/4I within a date range across ALL Buyers/
    // Orders, grouped by ItemCode alone - legacy has no Buyer/Order scope on this
    // screen at all. Reporting Currency/Price is picked from whichever
    // OrderwiseStockMaster row happens to match the item code first (buyer/order-
    // agnostic), mirroring legacy's own "seek temp->item_cd" behavior exactly (an
    // inherent limitation of an item-level report with no buyer/order scope, not a
    // bug worth "fixing" the way Stock Summary's double-counting was).
    //
    // Uses "GR" (not legacy's "0G") for Goods Received - this codebase's own
    // established Orderwise GRN type code (GoodsReceivedNoteService,
    // StockMovementItemReportService already use "GR" consistently; only General
    // Inventory's GRN kept legacy's literal "0G").
    public class StockValuationMonthlyReportService : IStockValuationMonthlyReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        public StockValuationMonthlyReportService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private async Task<(StockValuationMonthlyReportHeaderServiceModel Header, List<StockValuationMonthlyReportLineServiceModel> Lines)> BuildAsync(
            DateOnly fromDate, DateOnly toDate)
        {
            if (fromDate > toDate)
                throw new InvalidOperationException("Given Date Range not found.");

            var transactions = await _apparelProDbContext.OrderwiseStockTransactions
                .AsNoTracking()
                .Where(t => (t.TransactionType == "GR" || t.TransactionType == "4I")
                    && t.TransactionDate >= fromDate.ToDateTime(TimeOnly.MinValue)
                    && t.TransactionDate <= toDate.ToDateTime(TimeOnly.MaxValue))
                .OrderBy(t => t.ItemCode)
                .ThenBy(t => t.Id)
                .ToListAsync();

            if (transactions.Count == 0)
                throw new InvalidOperationException("No transactions to print.");

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var itemCodes = transactions.Select(t => t.ItemCode.Trim()).Distinct().ToList();

            // One (arbitrary) OrderwiseStockMaster row per item code - legacy's own
            // "seek temp->item_cd" against an index keyed item_cd+buyer+order picks
            // whichever buyer/order sorts first; EF has no direct equivalent of that
            // exact tie-break, so this takes the first match per item code instead
            // (same buyer/order-agnostic imprecision, just a different arbitrary pick).
            var allMasters = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => itemCodes.Contains(m.ItemCode))
                .ToListAsync();
            var masterByItemCode = allMasters
                .GroupBy(m => m.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var costProfiles = new List<ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile>();
            var buyerOrderPairs = transactions.Select(t => (t.BuyerCode, Order: t.Order.Trim())).Distinct().ToList();
            foreach (var (buyerCode, order) in buyerOrderPairs)
            {
                var rows = await _apparelProDbContext.StyleMaterialCostProfiles
                    .AsNoTracking()
                    .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                    .ToListAsync();
                costProfiles.AddRange(rows);
            }
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var baseItemCodes = itemCodes.Select(c => DecomposePart(c, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var lines = new List<StockValuationMonthlyReportLineServiceModel>();
            decimal grandReceivedValue = 0, grandIssuedValue = 0;

            foreach (var group in transactions.GroupBy(t => t.ItemCode.Trim()))
            {
                var itemCode = group.Key;
                masterByItemCode.TryGetValue(itemCode, out var master);
                string reportingCurrency = master?.Currency ?? "USD";
                string reportingUnit = master?.Unit ?? group.First().Unit;
                decimal reportingPrice = master?.Price ?? 0;

                decimal receivedQty = 0, receivedValue = 0, issuedQty = 0, issuedValue = 0;
                foreach (var tx in group)
                {
                    decimal lineValue = tx.Price.HasValue
                        ? await _currencyConversionService.ConvertAsync(tx.Price.Value * tx.Quantity, tx.Currency ?? reportingCurrency, reportingCurrency)
                        : 0;

                    if (tx.TransactionType == "GR")
                    {
                        receivedQty += tx.Quantity;
                        receivedValue += lineValue;
                    }
                    else
                    {
                        issuedQty += tx.Quantity;
                        issuedValue += lineValue;
                    }
                }

                profileByItemCode.TryGetValue(itemCode, out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                lines.Add(new StockValuationMonthlyReportLineServiceModel
                {
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = reportingUnit,
                    Currency = reportingCurrency,
                    UnitPrice = reportingPrice,
                    ReceivedQuantity = receivedQty,
                    ReceivedValue = receivedValue,
                    IssuedQuantity = issuedQty,
                    IssuedValue = issuedValue,
                });

                grandReceivedValue += receivedValue;
                grandIssuedValue += issuedValue;
            }

            var header = new StockValuationMonthlyReportHeaderServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalLineItems = lines.Count,
                TotalReceivedValue = grandReceivedValue,
                TotalIssuedValue = grandIssuedValue,
            };

            return (header, lines.OrderBy(l => l.ItemCode).ToList());
        }

        public async Task<StockValuationMonthlyReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (header, _) = await BuildAsync(fromDate, toDate);
            return header;
        }

        public async Task<List<StockValuationMonthlyReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate)
        {
            var (_, lines) = await BuildAsync(fromDate, toDate);
            return lines;
        }
    }
}
