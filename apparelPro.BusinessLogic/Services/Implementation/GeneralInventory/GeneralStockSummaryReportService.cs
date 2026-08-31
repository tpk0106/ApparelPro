using apparelPro.BusinessLogic.Services.interfaces.GeneralInventory;
using apparelPro.BusinessLogic.Services.Models.GeneralInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.GeneralInventory
{
    // Modern equivalent of legacy GI_STVAL.PRG - "SUMMARY OF STOCK VALUATION REPORT".
    // Groups every (Store, Item) balance as of a given month-end by Stock Type (the
    // first 2 characters of the 22-char composite ItemCode, looked up against the
    // Stocks reference table) then by Store, valuing each balance at the item's
    // CURRENT unit price (Value / QtyInHand off GeneralStockMasters, same as legacy's
    // own gi_stmst read) and converting into two chosen currencies.
    //
    // Deliberate deviation from GI_STVAL.PRG: legacy accumulates cf_bal across every
    // monthly gi_monst row up to the target month for a given item/store - since
    // gi_monst is itself a running balance per month, that sums several months' already-
    // cumulative closing balances together, which double-counts. This service instead
    // computes the single correct balance AS OF month-end via the same chronological-
    // replay technique as GeneralStockStatusReportService (this system has no gi_monst
    // equivalent by design), which is what the report is clearly trying to show.
    public class GeneralStockSummaryReportService : IGeneralStockSummaryReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        private static readonly string[] MovementTypeCodes =
        {
            "0G", "4I", "1TG", "6TG", "1TO", "6TO", "2R", "7SR", "7SD", "6D", "3A"
        };

        public GeneralStockSummaryReportService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private async Task<(GeneralStockSummaryReportHeaderServiceModel Header, List<GeneralStockSummaryReportLineServiceModel> Lines)> BuildAsync(
            int month, int year, string currency1, string currency2)
        {
            currency1 = currency1.Trim().ToUpper();
            currency2 = currency2.Trim().ToUpper();

            bool currency1Exists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == currency1);
            if (!currency1Exists)
                throw new KeyNotFoundException($"Invalid currency code '{currency1}'.");

            bool currency2Exists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == currency2);
            if (!currency2Exists)
                throw new KeyNotFoundException($"Invalid currency code '{currency2}'.");

            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            bool hasActivityThisMonth = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .AnyAsync(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd);
            if (!hasActivityThisMonth)
                throw new InvalidOperationException("Given month's transactions are not available.");

            var transactions = await _apparelProDbContext.GeneralStockTransactions
                .AsNoTracking()
                .Where(t => MovementTypeCodes.Contains(t.TransactionTypeCode) && t.TransactionDate <= monthEnd)
                .OrderBy(t => t.StoreCode)
                .ThenBy(t => t.ItemCode)
                .ThenBy(t => t.TransactionDate)
                .ThenBy(t => t.TransactionTime)
                .ThenBy(t => t.Id)
                .ToListAsync();

            // Month-end balance per (StoreCode, ItemCode) - same replay logic as
            // GeneralStockStatusReportService, just without the per-month totals it also
            // tracks (only the final running balance matters here).
            var balances = new List<(string StoreCode, string ItemCode, decimal Balance)>();
            foreach (var group in transactions.GroupBy(t => (t.StoreCode, t.ItemCode)))
            {
                decimal running = 0;
                foreach (var tx in group) // already chronological
                {
                    running = tx.TransactionTypeCode switch
                    {
                        "0G" or "1TG" or "1TO" or "2R" => running + tx.Quantity,
                        "3A" => tx.Quantity,
                        _ => running - tx.Quantity, // 4I, 6TG, 6TO, 7SR, 7SD, 6D
                    };
                }
                if (running != 0)
                    balances.Add((group.Key.StoreCode, group.Key.ItemCode, running));
            }

            var masters = await _apparelProDbContext.GeneralStockMasters.AsNoTracking().ToDictionaryAsync(m => (m.StoreCode, m.ItemCode));
            var storeDescriptions = await _apparelProDbContext.GeneralStores.AsNoTracking().ToDictionaryAsync(s => s.Code, s => s.Description);
            var stockTypeDescriptions = await _apparelProDbContext.Stocks.AsNoTracking().ToDictionaryAsync(s => s.StockCode, s => s.Description);

            // (StockTypeCode, StoreCode) -> accumulated value in each currency.
            var groupTotals = new Dictionary<(string StockTypeCode, string StoreCode), (decimal Value1, decimal Value2)>();

            foreach (var (storeCode, itemCode, balance) in balances)
            {
                if (!masters.TryGetValue((storeCode, itemCode), out var master))
                    continue; // no master row (e.g. deleted since) - nothing to price this against

                decimal unitPrice = master.QtyInHand != 0 ? Math.Round(master.Value / master.QtyInHand, 4) : 0;
                decimal valueInItemCurrency = unitPrice * balance;

                decimal value1 = await _currencyConversionService.ConvertAsync(valueInItemCurrency, master.Currency, currency1);
                decimal value2 = await _currencyConversionService.ConvertAsync(valueInItemCurrency, master.Currency, currency2);

                string stockTypeCode = itemCode.Length >= 2 ? itemCode.Substring(0, 2) : itemCode;
                var key = (stockTypeCode, storeCode);
                groupTotals.TryGetValue(key, out var existing);
                groupTotals[key] = (existing.Value1 + value1, existing.Value2 + value2);
            }

            var lines = new List<GeneralStockSummaryReportLineServiceModel>();
            decimal grandTotal1 = 0, grandTotal2 = 0;

            var stockTypeGroups = groupTotals
                .GroupBy(kv => kv.Key.StockTypeCode)
                .OrderBy(g => g.Key);

            foreach (var stockTypeGroup in stockTypeGroups)
            {
                decimal subTotal1 = 0, subTotal2 = 0;
                foreach (var storeEntry in stockTypeGroup.OrderBy(kv => kv.Key.StoreCode))
                {
                    lines.Add(new GeneralStockSummaryReportLineServiceModel
                    {
                        StockTypeCode = stockTypeGroup.Key,
                        StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                        StoreCode = storeEntry.Key.StoreCode,
                        StoreDescription = storeDescriptions.GetValueOrDefault(storeEntry.Key.StoreCode, ""),
                        ValueInCurrency1 = storeEntry.Value.Value1,
                        ValueInCurrency2 = storeEntry.Value.Value2,
                        RowType = "Store",
                    });
                    subTotal1 += storeEntry.Value.Value1;
                    subTotal2 += storeEntry.Value.Value2;
                }

                lines.Add(new GeneralStockSummaryReportLineServiceModel
                {
                    StockTypeCode = stockTypeGroup.Key,
                    StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                    ValueInCurrency1 = subTotal1,
                    ValueInCurrency2 = subTotal2,
                    RowType = "StockTypeSubtotal",
                });

                grandTotal1 += subTotal1;
                grandTotal2 += subTotal2;
            }

            lines.Add(new GeneralStockSummaryReportLineServiceModel
            {
                ValueInCurrency1 = grandTotal1,
                ValueInCurrency2 = grandTotal2,
                RowType = "GrandTotal",
            });

            var header = new GeneralStockSummaryReportHeaderServiceModel
            {
                Month = month,
                Year = year,
                Currency1 = currency1,
                Currency2 = currency2,
                GrandTotalCurrency1 = grandTotal1,
                GrandTotalCurrency2 = grandTotal2,
                TotalStockTypes = stockTypeGroups.Count(),
            };

            return (header, lines);
        }

        public async Task<GeneralStockSummaryReportHeaderServiceModel> GetHeaderAsync(int month, int year, string currency1, string currency2)
        {
            var (header, _) = await BuildAsync(month, year, currency1, currency2);
            return header;
        }

        public async Task<List<GeneralStockSummaryReportLineServiceModel>> GetLinesAsync(int month, int year, string currency1, string currency2)
        {
            var (_, lines) = await BuildAsync(month, year, currency1, currency2);
            return lines;
        }
    }
}
