using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_STBAL.PRG - "ITEM-WISE STOCK BALANCES"
    // (Orderwise). System-wide (every Buyer/Order), filtered to a 6-char Stock+Item
    // code range, three-level grouping: Stock Type (2-char) -> 6-char Item group ->
    // individual (Buyer, Order, Item) lines. Zero-balance items are excluded
    // entirely - legacy: "dele all for bal_qty = 0". Balance is derived using the
    // same formula as StockMovementReportService/StockValuationReportService (this
    // system never persists a mutable bal_qty). Reporting currency is picked from
    // the first matching row (arbitrary), same imprecision already documented on
    // StockValuationMonthlyReportService (legacy has no single well-defined currency
    // for a report that spans every Buyer/Order).
    public class ItemWiseStockBalanceService : IItemWiseStockBalanceService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        public ItemWiseStockBalanceService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private static string SixCharRange(string itemCode) =>
            itemCode.Length >= 6 ? itemCode.Substring(0, 6) : itemCode.PadRight(6);

        private async Task<(ItemWiseStockBalanceHeaderServiceModel Header, List<ItemWiseStockBalanceLineServiceModel> Lines)> BuildAsync(
            string fromRange, string toRange)
        {
            fromRange = fromRange.Trim().ToUpper();
            toRange = toRange.Trim().ToUpper();
            if (string.Compare(fromRange, toRange, StringComparison.Ordinal) > 0)
                throw new InvalidOperationException("Given Stock/Item range not found.");

            var allMasters = await _apparelProDbContext.OrderwiseStockMasters.AsNoTracking().ToListAsync();

            var inRange = allMasters
                .Where(m => string.Compare(SixCharRange(m.ItemCode), fromRange, StringComparison.Ordinal) >= 0
                    && string.Compare(SixCharRange(m.ItemCode), toRange, StringComparison.Ordinal) <= 0)
                .ToList();

            var reportingCurrency = inRange.FirstOrDefault()?.Currency ?? "USD";

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var costProfiles = new List<ApparelPro.Data.Models.OrderManagement.MaterialConsumption.StyleMaterialCostProfile>();
            var buyerOrderPairs = inRange.Select(m => (m.BuyerCode, Order: m.Order.Trim())).Distinct().ToList();
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

            var baseItemCodes = inRange.Select(m => DecomposePart(m.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var itemGroupCodes = inRange.Select(m => SixCharRange(m.ItemCode)).Distinct().ToList();
            var itemGroupDescriptions = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => itemGroupCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var stockTypeCodes = inRange.Select(m => DecomposePart(m.ItemCode, 0, 2)).Distinct().ToList();
            var stockTypeDescriptions = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => stockTypeCodes.Contains(s.StockCode))
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var lines = new List<ItemWiseStockBalanceLineServiceModel>();
            decimal grandReceivedValue = 0, grandBalanceValue = 0;

            var stockTypeGroups = inRange
                .Select(m => new { Master = m, StockTypeCode = DecomposePart(m.ItemCode, 0, 2), ItemGroupCode = SixCharRange(m.ItemCode) })
                .GroupBy(x => x.StockTypeCode)
                .OrderBy(g => g.Key);

            foreach (var stockTypeGroup in stockTypeGroups)
            {
                decimal stockTypeReceivedValue = 0, stockTypeBalanceValue = 0;

                var itemGroups = stockTypeGroup.GroupBy(x => x.ItemGroupCode).OrderBy(g => g.Key);
                foreach (var itemGroup in itemGroups)
                {
                    decimal groupReceivedValue = 0, groupBalanceValue = 0;
                    bool groupHasAnyLine = false;

                    foreach (var entry in itemGroup.OrderBy(x => x.Master.BuyerCode).ThenBy(x => x.Master.Order))
                    {
                        var master = entry.Master;
                        decimal balance = master.ReceivedQuantity - master.IssuedQuantity + master.ReturnedQuantity
                            + master.TransferInQuantity - master.TransferOutQuantity - master.DamagedQuantity
                            - master.SupplierReturnQuantity - master.AdditionalIssuedQuantity;

                        if (balance == 0)
                            continue; // legacy: "dele all for bal_qty = 0"

                        groupHasAnyLine = true;
                        var itemCode = master.ItemCode.Trim();
                        var order = master.Order.Trim();

                        profileByBuyerOrderItem.TryGetValue((master.BuyerCode, order, itemCode), out var matchingProfile);
                        var description = matchingProfile?.Description?.Trim();
                        if (string.IsNullOrWhiteSpace(description))
                            catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                        decimal unitPrice = await _currencyConversionService.ConvertAsync(master.Price, master.Currency, reportingCurrency);
                        decimal receivedValue = unitPrice * master.ReceivedQuantity;
                        decimal balanceValue = unitPrice * balance;

                        lines.Add(new ItemWiseStockBalanceLineServiceModel
                        {
                            StockTypeCode = entry.StockTypeCode,
                            StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(entry.StockTypeCode, ""),
                            ItemGroupCode = entry.ItemGroupCode,
                            ItemGroupDescription = itemGroupDescriptions.GetValueOrDefault(entry.ItemGroupCode, ""),
                            BuyerCode = master.BuyerCode,
                            Order = order,
                            ItemCode = itemCode,
                            Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                            Unit = master.Unit,
                            UnitPrice = unitPrice,
                            ReceivedQuantity = master.ReceivedQuantity,
                            ReceivedValue = receivedValue,
                            QtyInHand = balance,
                            BalanceValue = balanceValue,
                            RowType = "Item",
                        });

                        groupReceivedValue += receivedValue;
                        groupBalanceValue += balanceValue;
                    }

                    if (!groupHasAnyLine)
                        continue;

                    lines.Add(new ItemWiseStockBalanceLineServiceModel
                    {
                        StockTypeCode = stockTypeGroup.Key,
                        StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                        ItemGroupCode = itemGroup.Key,
                        ItemGroupDescription = itemGroupDescriptions.GetValueOrDefault(itemGroup.Key, ""),
                        ReceivedValue = groupReceivedValue,
                        BalanceValue = groupBalanceValue,
                        RowType = "ItemGroupSubtotal",
                    });

                    stockTypeReceivedValue += groupReceivedValue;
                    stockTypeBalanceValue += groupBalanceValue;
                }

                lines.Add(new ItemWiseStockBalanceLineServiceModel
                {
                    StockTypeCode = stockTypeGroup.Key,
                    StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                    ReceivedValue = stockTypeReceivedValue,
                    BalanceValue = stockTypeBalanceValue,
                    RowType = "StockTypeSubtotal",
                });

                grandReceivedValue += stockTypeReceivedValue;
                grandBalanceValue += stockTypeBalanceValue;
            }

            lines.Add(new ItemWiseStockBalanceLineServiceModel
            {
                ReceivedValue = grandReceivedValue,
                BalanceValue = grandBalanceValue,
                RowType = "GrandTotal",
            });

            int totalLineItems = lines.Count(l => l.RowType == "Item");
            if (totalLineItems == 0)
                throw new InvalidOperationException("Given Stock/Item range not found.");

            var header = new ItemWiseStockBalanceHeaderServiceModel
            {
                FromRange = fromRange,
                ToRange = toRange,
                Currency = reportingCurrency,
                TotalLineItems = totalLineItems,
                TotalReceivedValue = grandReceivedValue,
                TotalBalanceValue = grandBalanceValue,
            };

            return (header, lines);
        }

        public async Task<ItemWiseStockBalanceHeaderServiceModel> GetHeaderAsync(string fromRange, string toRange)
        {
            var (header, _) = await BuildAsync(fromRange, toRange);
            return header;
        }

        public async Task<List<ItemWiseStockBalanceLineServiceModel>> GetLinesAsync(string fromRange, string toRange)
        {
            var (_, lines) = await BuildAsync(fromRange, toRange);
            return lines;
        }
    }
}
