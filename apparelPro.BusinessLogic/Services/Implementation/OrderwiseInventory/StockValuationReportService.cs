using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SVAL.PRG - "STOCK VALUATION REPORT" (Orderwise).
    // A per-Buyer/Order valuation grouped by Stock Type (first 2 chars of the 22-char
    // composite ItemCode), reading OrderwiseStockMaster's live running totals directly -
    // no month-based replay needed, unlike General Inventory's own Stock Status/Movement/
    // Summary reports (this one has no month/date dimension in legacy at all). Balance
    // is derived using the exact same formula as StockMovementReportService, for
    // consistency (this system never persists a mutable bal_qty).
    //
    // Semi-finished garment items are excluded entirely - legacy: "seek
    // xbuyer+xorder+item_cd on od_aitm; if found() .and. semi_fin -> skip".
    public class StockValuationReportService : IStockValuationReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ICurrencyConversionService _currencyConversionService;

        public StockValuationReportService(ApparelProDbContext apparelProDbContext, ICurrencyConversionService currencyConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _currencyConversionService = currencyConversionService;
        }

        private async Task<(StockValuationReportHeaderServiceModel Header, List<StockValuationReportLineServiceModel> Lines)> BuildAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (purchaseOrder == null)
                throw new KeyNotFoundException($"Buyer/Order not found in P/O Master File (Buyer: {buyerCode}, Order: {order}).");

            var masters = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .ToListAsync();
            if (masters.Count == 0)
                throw new InvalidOperationException("No Items Assigned to above Order.");

            var assignments = await _apparelProDbContext.GarmentAdditionalCosts
                .AsNoTracking()
                .Where(g => g.BuyerCode == buyerCode && g.Order == order && g.IsSemiFinishedGarment)
                .Select(g => g.ItemCode)
                .Distinct()
                .ToListAsync();
            var semiFinishedItemCodes = new HashSet<string>(assignments);

            masters = masters.Where(m => !semiFinishedItemCodes.Contains(m.ItemCode)).ToList();

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = masters.Select(m => DecomposePart(m.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var stockTypeCodes = masters.Select(m => DecomposePart(m.ItemCode, 0, 2)).Distinct().ToList();
            var stockTypeDescriptions = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => stockTypeCodes.Contains(s.StockCode))
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var lines = new List<StockValuationReportLineServiceModel>();
            decimal grandReceivedValue = 0, grandIssuedValue = 0, grandBalanceValue = 0;

            var stockTypeGroups = masters
                .Select(m => new { Master = m, StockTypeCode = DecomposePart(m.ItemCode, 0, 2) })
                .GroupBy(x => x.StockTypeCode)
                .OrderBy(g => g.Key);

            foreach (var stockTypeGroup in stockTypeGroups)
            {
                decimal subReceivedValue = 0, subIssuedValue = 0, subBalanceValue = 0;

                foreach (var entry in stockTypeGroup.OrderBy(x => x.Master.ItemCode))
                {
                    var master = entry.Master;
                    var itemCode = master.ItemCode.Trim();

                    profileByItemCode.TryGetValue(itemCode, out var matchingProfile);
                    var description = matchingProfile?.Description?.Trim();
                    if (string.IsNullOrWhiteSpace(description))
                        catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                    decimal balance = master.ReceivedQuantity - master.IssuedQuantity + master.ReturnedQuantity
                        + master.TransferInQuantity - master.TransferOutQuantity - master.DamagedQuantity
                        - master.SupplierReturnQuantity - master.AdditionalIssuedQuantity;

                    decimal unitPrice = await _currencyConversionService.ConvertAsync(master.Price, master.Currency, purchaseOrder.CurrencyCode);
                    decimal receivedValue = unitPrice * master.ReceivedQuantity;
                    decimal issuedValue = unitPrice * master.IssuedQuantity;
                    decimal balanceValue = unitPrice * balance;

                    lines.Add(new StockValuationReportLineServiceModel
                    {
                        StockTypeCode = entry.StockTypeCode,
                        StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(entry.StockTypeCode, ""),
                        ItemCode = itemCode,
                        Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                        Unit = master.Unit,
                        UnitPrice = unitPrice,
                        OrderedQuantity = master.OrderedQuantity,
                        ReceivedQuantity = master.ReceivedQuantity,
                        ReceivedValue = receivedValue,
                        IssuedQuantity = master.IssuedQuantity,
                        IssuedValue = issuedValue,
                        QtyInHand = balance,
                        BalanceValue = balanceValue,
                        RowType = "Item",
                    });

                    subReceivedValue += receivedValue;
                    subIssuedValue += issuedValue;
                    subBalanceValue += balanceValue;
                }

                lines.Add(new StockValuationReportLineServiceModel
                {
                    StockTypeCode = stockTypeGroup.Key,
                    StockTypeDescription = stockTypeDescriptions.GetValueOrDefault(stockTypeGroup.Key, ""),
                    ReceivedValue = subReceivedValue,
                    IssuedValue = subIssuedValue,
                    BalanceValue = subBalanceValue,
                    RowType = "StockTypeSubtotal",
                });

                grandReceivedValue += subReceivedValue;
                grandIssuedValue += subIssuedValue;
                grandBalanceValue += subBalanceValue;
            }

            lines.Add(new StockValuationReportLineServiceModel
            {
                ReceivedValue = grandReceivedValue,
                IssuedValue = grandIssuedValue,
                BalanceValue = grandBalanceValue,
                RowType = "GrandTotal",
            });

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            var styles = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .Select(s => new StockValuationReportStyleServiceModel
                {
                    StyleCode = s.StyleCode,
                    Quantity = s.Quantity ?? 0,
                    Unit = s.Unit ?? "",
                    UnitPrice = s.UnitPrice ?? 0,
                })
                .ToListAsync();

            var header = new StockValuationReportHeaderServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : buyerCode.ToString(),
                Order = order,
                Currency = purchaseOrder.CurrencyCode,
                Styles = styles,
                TotalReceivedValue = grandReceivedValue,
                TotalIssuedValue = grandIssuedValue,
                TotalBalanceValue = grandBalanceValue,
            };

            return (header, lines);
        }

        public async Task<StockValuationReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order)
        {
            var (header, _) = await BuildAsync(buyerCode, order);
            return header;
        }

        public async Task<List<StockValuationReportLineServiceModel>> GetLinesAsync(int buyerCode, string order)
        {
            var (_, lines) = await BuildAsync(buyerCode, order);
            return lines;
        }
    }
}
