using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_SSTAT.PRG - "STOCK STATUS REPORT" (Orderwise).
    // A per-Buyer/Order current-snapshot listing straight off OrderwiseStock - no
    // month/date filter, no transaction replay (same family as
    // GeneralStockValuationReportService / GeneralStockReorderReportService).
    public class StockStatusReportService : IStockStatusReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public StockStatusReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private async Task<(StockStatusReportHeaderServiceModel Header, List<StockStatusReportLineServiceModel> Lines)> BuildAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            var purchaseOrderExists = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .AnyAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (!purchaseOrderExists)
                throw new KeyNotFoundException($"Buyer/Order not found in P/O Master File (Buyer: {buyerCode}, Order: {order}).");

            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .OrderBy(s => s.ItemCode)
                .ToListAsync();
            if (stockRows.Count == 0)
                throw new InvalidOperationException("No Items Assigned to above Order.");

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            static string DecomposePart(string fullItemCode, int start, int length) =>
                fullItemCode.Length >= start + length ? fullItemCode.Substring(start, length).Trim() : string.Empty;

            var baseItemCodes = stockRows.Select(s => DecomposePart(s.ItemCode, 2, 4)).Distinct().ToList();
            var catalogDescriptionByItemCode = await _apparelProDbContext.StockItems
                .AsNoTracking()
                .Where(c => baseItemCodes.Contains(c.ItemCode.Trim()))
                .ToDictionaryAsync(c => c.ItemCode.Trim(), c => c.Description);

            var lines = stockRows.Select(s =>
            {
                var itemCode = s.ItemCode.Trim();
                profileByItemCode.TryGetValue(itemCode, out var matchingProfile);
                var description = matchingProfile?.Description?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    catalogDescriptionByItemCode.TryGetValue(DecomposePart(itemCode, 2, 4), out description);

                return new StockStatusReportLineServiceModel
                {
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(description) ? description!.Trim() : "(No description available)",
                    Unit = s.Unit,
                    OrderedQuantity = s.OrderedQuantity,
                    ReceivedQuantity = s.ToDateReceived,
                    BalanceToReceive = s.OrderedQuantity - s.ToDateReceived,
                    DamagedQuantity = s.DamagedQuantity,
                    QtyInHand = s.QtyInHand,
                    StoreCode = s.StoreCode,
                };
            }).ToList();

            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            var header = new StockStatusReportHeaderServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = !string.IsNullOrWhiteSpace(buyerName) ? buyerName : buyerCode.ToString(),
                Order = order,
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<StockStatusReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order)
        {
            var (header, _) = await BuildAsync(buyerCode, order);
            return header;
        }

        public async Task<List<StockStatusReportLineServiceModel>> GetLinesAsync(int buyerCode, string order)
        {
            var (_, lines) = await BuildAsync(buyerCode, order);
            return lines;
        }
    }
}
