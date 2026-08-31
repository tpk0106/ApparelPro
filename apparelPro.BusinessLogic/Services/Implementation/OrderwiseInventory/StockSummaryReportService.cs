using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL2.PRG - "SUMMARY OF STOCK VALUE - Order-wise
    // Inventory" (a.k.a. "Stock Summary Report - Basis wise" in IN_MENU.PRG). Groups
    // every OrderwiseStock row (system-wide, every Buyer/Order) by Stock Type then
    // Store, valuing each row's CURRENT QtyInHand (a live snapshot - unlike General
    // Inventory's own Stock Summary Report, this one needs no chronological replay:
    // legacy reads in_stock->qty_in_hd directly, not a running-balance derivation)
    // in two chosen currencies. Price/Currency come from the matching
    // OrderwiseStockMaster row (Buyer+Order+Item); if none exists, the row
    // contributes 0 value rather than reproducing legacy's undefined EOF-record read.
    //
    // A store row is only counted if the STORE's own contribution to BOTH totals is
    // non-negative (mirroring legacy's own gate below) - fully matching legacy's
    // stricter "> 0 .and. > 0" only for whether the row is individually PRINTED, not
    // whether it feeds the subtotal (legacy always feeds the subtotal regardless).
    public class StockSummaryReportService : IStockSummaryReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        public StockSummaryReportService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private async Task<(StockSummaryReportHeaderServiceModel Header, List<StockSummaryReportLineServiceModel> Lines)> BuildAsync(
            string currency1, string currency2)
        {
            currency1 = currency1.Trim().ToUpper();
            currency2 = currency2.Trim().ToUpper();

            bool currency1Exists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == currency1);
            if (!currency1Exists)
                throw new KeyNotFoundException($"Invalid currency code '{currency1}'.");

            bool currency2Exists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == currency2);
            if (!currency2Exists)
                throw new KeyNotFoundException($"Invalid currency code '{currency2}'.");

            var stockRows = await _apparelProDbContext.OrderwiseStocks.AsNoTracking().ToListAsync();
            if (stockRows.Count == 0)
                throw new InvalidOperationException("Given month's transactions are not available.");

            var masterKeys = stockRows.Select(s => (s.BuyerCode, Order: s.Order.Trim(), ItemCode: s.ItemCode.Trim())).Distinct().ToList();
            var allMasters = await _apparelProDbContext.OrderwiseStockMasters.AsNoTracking().ToListAsync();
            var masterByKey = allMasters
                .GroupBy(m => (m.BuyerCode, Order: m.Order.Trim(), ItemCode: m.ItemCode.Trim()))
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var stockTypeCodes = stockRows.Select(s => DecomposePart(s.ItemCode, 0, 2)).Distinct().ToList();
            var stockTypeDescriptions = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => stockTypeCodes.Contains(s.StockCode))
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            // (StockTypeCode, StoreCode) -> accumulated value in each currency.
            var groupTotals = new Dictionary<(string StockTypeCode, string StoreCode), (decimal Value1, decimal Value2)>();

            foreach (var stock in stockRows)
            {
                if (stock.QtyInHand == 0)
                    continue;

                var key = (stock.BuyerCode, Order: stock.Order.Trim(), ItemCode: stock.ItemCode.Trim());
                decimal value1 = 0, value2 = 0;
                if (masterByKey.TryGetValue(key, out var master))
                {
                    value1 = await _currencyConversionService.ConvertAsync(master.Price * stock.QtyInHand, master.Currency, currency1);
                    value2 = await _currencyConversionService.ConvertAsync(master.Price * stock.QtyInHand, master.Currency, currency2);
                }

                string stockTypeCode = DecomposePart(stock.ItemCode, 0, 2);
                var groupKey = (stockTypeCode, stock.StoreCode);
                groupTotals.TryGetValue(groupKey, out var existing);
                groupTotals[groupKey] = (existing.Value1 + value1, existing.Value2 + value2);
            }

            var lines = new List<StockSummaryReportLineServiceModel>();
            decimal grandTotal1 = 0, grandTotal2 = 0;

            var stockTypeGroups = groupTotals
                .GroupBy(kv => kv.Key.StockTypeCode)
                .OrderBy(g => g.Key);

            foreach (var stockTypeGroup in stockTypeGroups)
            {
                decimal subTotal1 = 0, subTotal2 = 0;
                foreach (var storeEntry in stockTypeGroup.OrderBy(kv => kv.Key.StoreCode))
                {
                    // Legacy only PRINTS the store row when both totals are positive,
                    // but always folds it into the subtotal either way.
                    if (storeEntry.Value.Value1 > 0 && storeEntry.Value.Value2 > 0)
                    {
                        lines.Add(new StockSummaryReportLineServiceModel
                        {
                            StockTypeCode = stockTypeGroup.Key,
                            StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                            StoreCode = storeEntry.Key.StoreCode,
                            ValueInCurrency1 = storeEntry.Value.Value1,
                            ValueInCurrency2 = storeEntry.Value.Value2,
                            RowType = "Store",
                        });
                    }
                    subTotal1 += storeEntry.Value.Value1;
                    subTotal2 += storeEntry.Value.Value2;
                }

                lines.Add(new StockSummaryReportLineServiceModel
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

            lines.Add(new StockSummaryReportLineServiceModel
            {
                ValueInCurrency1 = grandTotal1,
                ValueInCurrency2 = grandTotal2,
                RowType = "GrandTotal",
            });

            var header = new StockSummaryReportHeaderServiceModel
            {
                Currency1 = currency1,
                Currency2 = currency2,
                GrandTotalCurrency1 = grandTotal1,
                GrandTotalCurrency2 = grandTotal2,
                TotalStockTypes = stockTypeGroups.Count(),
            };

            return (header, lines);
        }

        public async Task<StockSummaryReportHeaderServiceModel> GetHeaderAsync(string currency1, string currency2)
        {
            var (header, _) = await BuildAsync(currency1, currency2);
            return header;
        }

        public async Task<List<StockSummaryReportLineServiceModel>> GetLinesAsync(string currency1, string currency2)
        {
            var (_, lines) = await BuildAsync(currency1, currency2);
            return lines;
        }
    }
}
