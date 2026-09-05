using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderwiseInventory
{
    // Modern equivalent of legacy IN_RMCON.PRG - "RAW MATERIAL CONTROL SHEET". Per-
    // Buyer/Order, using StyleMaterialConsumptionLedger (od_sacc3) aggregated across
    // every style/color/size sharing the same raw-material composite ItemCode
    // (StockCode+ItemCode+Feature1-4), compared against OrderwiseStockMaster's live
    // Order/Received/Issued/Balance totals for that same item. "Exact Consumption"
    // strips out the allowance percentage: legacy's own formula
    // `tot_con - (tot_con/(100+perc_all))*perc_all`, replicated verbatim.
    public class RawMaterialControlSheetService : IRawMaterialControlSheetService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISharedService _sharedService;

        public RawMaterialControlSheetService(ApparelProDbContext apparelProDbContext, ISharedService sharedService)
        {
            _apparelProDbContext = apparelProDbContext;
            _sharedService = sharedService;
        }

        private async Task<(RawMaterialControlSheetHeaderServiceModel Header, List<RawMaterialControlSheetLineServiceModel> Lines)> BuildAsync(
            int buyerCode, string order)
        {
            order = order.Trim();

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.BuyerCode == buyerCode && po.Order == order);
            if (purchaseOrder == null)
                throw new KeyNotFoundException($"Buyer/Order not found in Order Confirmation (Buyer: {buyerCode}, Order: {order}).");

            var buyer = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            var ledgerRows = await _apparelProDbContext.StyleMaterialConsumptionLedgers
                .AsNoTracking()
                .Where(l => l.BuyerCode == buyerCode && l.Order == order)
                .ToListAsync();
            if (ledgerRows.Count == 0)
                throw new InvalidOperationException("No Material Consumptions for given Buyer/Order.");

            var costProfiles = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(p => p.BuyerCode == buyerCode && p.Order.Trim() == order)
                .ToListAsync();
            var profileByItemCode = costProfiles
                .GroupBy(p => p.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var masters = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .ToListAsync();
            var masterByItemCode = masters
                .GroupBy(m => m.ItemCode.Trim())
                .ToDictionary(g => g.Key, g => g.First());

            var stockCodes = ledgerRows.Select(l => l.StockCode.Trim()).Distinct().ToList();
            var stockDescriptions = await _apparelProDbContext.Stocks
                .AsNoTracking()
                .Where(s => stockCodes.Contains(s.StockCode))
                .ToDictionaryAsync(s => s.StockCode, s => s.Description);

            var lines = new List<RawMaterialControlSheetLineServiceModel>();

            var itemGroups = ledgerRows
                .GroupBy(l => l.StockCode.Trim() + l.ItemCode.Trim() + l.Feature1.Trim() + l.Feature2.Trim() + l.Feature3.Trim() + l.Feature4.Trim())
                .OrderBy(g => g.Key);

            foreach (var group in itemGroups)
            {
                var first = group.First();
                string stockCode = first.StockCode.Trim();
                string itemCode = group.Key;
                string reportingUnit = first.ItemUnit;

                decimal totalConsumption = 0, exactConsumption = 0;
                foreach (var row in group)
                {
                    decimal converted = await _sharedService.ConvertUnitAsync(row.ItemUnit, reportingUnit, row.TotalConsumption);
                    totalConsumption += converted;
                    exactConsumption += converted - (converted / (100 + row.PercentageAllowance)) * row.PercentageAllowance;
                }

                profileByItemCode.TryGetValue(itemCode, out var profile);
                masterByItemCode.TryGetValue(itemCode, out var master);

                decimal orderQty = 0, receivedQty = 0, issuedQty = 0, balance = 0, unitPrice = 0;
                string currency = "";
                if (master != null)
                {
                    orderQty = await _sharedService.ConvertUnitAsync(master.Unit, reportingUnit, master.OrderedQuantity);
                    receivedQty = await _sharedService.ConvertUnitAsync(master.Unit, reportingUnit, master.ReceivedQuantity);
                    issuedQty = await _sharedService.ConvertUnitAsync(master.Unit, reportingUnit, master.IssuedQuantity);
                    decimal masterBalance = master.ReceivedQuantity - master.IssuedQuantity + master.ReturnedQuantity
                        + master.TransferInQuantity - master.TransferOutQuantity - master.DamagedQuantity
                        - master.SupplierReturnQuantity - master.AdditionalIssuedQuantity;
                    balance = await _sharedService.ConvertUnitAsync(master.Unit, reportingUnit, masterBalance);
                    unitPrice = master.Price;
                    currency = master.Currency;
                }

                lines.Add(new RawMaterialControlSheetLineServiceModel
                {
                    StockCode = stockCode,
                    StockDescription = stockDescriptions.GetValueOrDefault(stockCode, ""),
                    ItemCode = itemCode,
                    Description = !string.IsNullOrWhiteSpace(profile?.Description) ? profile!.Description.Trim() : "(No description available)",
                    Unit = reportingUnit,
                    TotalConsumption = totalConsumption,
                    ExactConsumption = exactConsumption,
                    TotalOrderQuantity = orderQty,
                    TotalReceivedQuantity = receivedQty,
                    TotalIssuedQuantity = issuedQty,
                    QtyInHand = balance,
                    UnitPrice = unitPrice,
                    Currency = currency,
                });
            }

            var header = new RawMaterialControlSheetHeaderServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyer?.Name ?? "",
                Order = order,
                ItemDescription = purchaseOrder.Description ?? "",
                OrderQuantity = purchaseOrder.TotalQuantity,
                Unit = purchaseOrder.UnitCode,
                TotalLineItems = lines.Count,
            };

            return (header, lines);
        }

        public async Task<RawMaterialControlSheetHeaderServiceModel> GetHeaderAsync(int buyerCode, string order)
        {
            var (header, _) = await BuildAsync(buyerCode, order);
            return header;
        }

        public async Task<List<RawMaterialControlSheetLineServiceModel>> GetLinesAsync(int buyerCode, string order)
        {
            var (_, lines) = await BuildAsync(buyerCode, order);
            return lines;
        }
    }
}
