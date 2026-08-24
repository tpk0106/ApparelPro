using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStockArrivalStatusReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    // Replicates OD_STARV.PRG's "STOCK ARRIVAL STATUS REPORT" - see the service model's
    // header comment for the one legacy defect deliberately not replicated (stale PO
    // header reuse across lines from different purchase orders).
    public class StockArrivalStatusReportService : IStockArrivalStatusReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public StockArrivalStatusReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<StockArrivalStatusReportServiceModel> GetStockArrivalStatusReportAsync(int buyerCode, string order, DateTime asOfDate)
        {
            order = order?.Trim() ?? string.Empty;

            if (buyerCode <= 0 || string.IsNullOrEmpty(order))
                throw new InvalidOperationException("Buyer Code and Order Code are both required.");

            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BuyerCode == buyerCode && p.Order == order);

            // Mirrors legacy's "Buyer/Order not found in P/O Master File" error box.
            if (purchaseOrder == null)
                throw new InvalidOperationException("Buyer/Order not found in P/O Master File.");

            var materialLines = await _apparelProDbContext.StyleMaterialCostProfiles
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order)
                .OrderBy(m => m.ItemCode)
                .ToListAsync();

            // Mirrors legacy's "No Trimmings Entered for above Order" error box.
            if (materialLines.Count == 0)
                throw new InvalidOperationException("No Trimmings Entered for above Order.");

            var buyer = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BuyerCode == buyerCode);

            // od_sacc2 rows are keyed Buyer+Order+Type+Style+ItemCode, so a single Order
            // can carry multiple Type/Style combos (one per style under it) - fetch every
            // PO line for this Buyer+Order up front, then match each material line to its
            // own Type+Style+ItemCode below rather than assuming one shared Type.
            var itemCodes = materialLines.Select(m => m.ItemCode).ToList();
            var poDetailLines = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .Where(d => d.Buyer == buyerCode && d.Order == order && itemCodes.Contains(d.ItemCode))
                .ToListAsync();

            var poNumbers = poDetailLines.Select(d => d.PONumber).Distinct().ToList();
            var poHeaders = await _apparelProDbContext.SupplierPurchaseOrders
                .AsNoTracking()
                .Where(h => poNumbers.Contains(h.PurchaseOrderNumber))
                .ToDictionaryAsync(h => h.PurchaseOrderNumber);

            var supplierCodes = poHeaders.Values.Select(h => h.SupplierCode).Distinct().ToList();
            var supplierNames = await _apparelProDbContext.Suppliers
                .AsNoTracking()
                .Where(s => supplierCodes.Contains(s.SupplierCode.ToString()))
                .ToDictionaryAsync(s => s.SupplierCode.ToString(), s => s.Name);

            var storeCodes = poHeaders.Values.Select(h => h.StoreCode).Distinct().ToList();
            var stockRows = await _apparelProDbContext.OrderwiseStocks
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order
                    && storeCodes.Contains(s.StoreCode) && itemCodes.Contains(s.ItemCode))
                .ToListAsync();

            var supplierReturnQuantities = await _apparelProDbContext.OrderwiseStockMasters
                .AsNoTracking()
                .Where(m => m.BuyerCode == buyerCode && m.Order == order && itemCodes.Contains(m.ItemCode))
                .ToDictionaryAsync(m => m.ItemCode, m => m.SupplierReturnQuantity);

            var conversionRates = await _apparelProDbContext.UnitConversion
                .AsNoTracking()
                .ToDictionaryAsync(c => (c.FromUnit, c.ToUnit), c => c.Measure ?? 1m);

            decimal ConvertQuantity(decimal qty, string fromUnit, string toUnit)
            {
                if (string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase))
                    return qty;
                // Best-effort: falls back to the unconverted quantity when no conversion
                // rate is on file, rather than failing the whole report over one missing
                // rate - matches this project's other reports' tolerance for reference
                // data gaps (e.g. missing GarmentType names still render, just blank).
                return conversionRates.TryGetValue((fromUnit, toUnit), out var rate) ? qty * rate : qty;
            }

            var items = materialLines.Select(material =>
            {
                var linesForItem = poDetailLines
                    .Where(d => d.Type == material.TypeCode && d.Style == material.StyleCode && d.ItemCode == material.ItemCode)
                    .ToList();

                var poLines = linesForItem.Select(line =>
                {
                    var header = poHeaders.GetValueOrDefault(line.PONumber);
                    var supplierName = header != null ? supplierNames.GetValueOrDefault(header.SupplierCode, "") : "";
                    var expectedDate = line.ExportDate; // legacy exp_date - see PODetailLineServiceModel note
                    return new StockArrivalPoLineServiceModel
                    {
                        PurchaseOrderNumber = line.PONumber,
                        OrderedQuantity = line.OrderQuantity,
                        StoreCode = header?.StoreCode ?? "",
                        SupplierName = supplierName,
                        ExpectedDate = expectedDate,
                        DelayDays = expectedDate.HasValue ? (asOfDate.Date - expectedDate.Value.Date).Days + 1 : null,
                        SupplierReturnQuantity = supplierReturnQuantities.GetValueOrDefault(material.ItemCode, 0),
                    };
                }).ToList();

                var distinctStoresForItem = linesForItem
                    .Select(l => poHeaders.GetValueOrDefault(l.PONumber)?.StoreCode)
                    .Where(s => s != null)
                    .Distinct()
                    .ToList();

                var totalReceived = stockRows
                    .Where(s => s.ItemCode == material.ItemCode && distinctStoresForItem.Contains(s.StoreCode))
                    .Sum(s => ConvertQuantity(s.ToDateReceived, s.Unit, material.ItemUnit));

                return new StockArrivalItemServiceModel
                {
                    ItemCode = material.ItemCode,
                    Description = material.Description,
                    Unit = material.ItemUnit,
                    OrderedQuantity = material.TotalConsumption,
                    TotalReceivedQuantity = totalReceived,
                    BalanceToReceive = material.TotalConsumption - totalReceived,
                    PurchaseOrderLines = poLines,
                };
            }).ToList();

            return new StockArrivalStatusReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyer?.Name ?? buyerCode.ToString(),
                Order = order,
                AsOfDate = asOfDate,
                TotalOrderQuantity = purchaseOrder.TotalQuantity,
                Unit = purchaseOrder.UnitCode ?? "",
                Items = items,
            };
        }
    }
}
