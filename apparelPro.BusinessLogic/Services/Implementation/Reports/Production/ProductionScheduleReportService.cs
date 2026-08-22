using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionScheduleReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_MSCHD.PRG's "PRODUCTION SCHEDULE" report.
    public class ProductionScheduleReportService : IProductionScheduleReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionScheduleReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ProductionScheduleReportServiceModel> GetProductionScheduleReportAsync(
            DateOnly fromDate, DateOnly toDate)
        {
            // Mirrors legacy's own guard ("From Date cannot be greater than To Date").
            if (fromDate > toDate)
                throw new InvalidOperationException("From Date cannot be greater than To Date.");

            var allocations = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.EstimatedStartDate >= fromDate && a.EstimatedStartDate <= toDate)
                .OrderBy(a => a.LineCode).ThenBy(a => a.EstimatedStartDate)
                .ThenBy(a => a.BuyerCode).ThenBy(a => a.Order)
                .ToListAsync();

            // Mirrors legacy's "Given date Range not found." error box.
            if (allocations.Count == 0)
                throw new InvalidOperationException("Given date range not found.");

            var buyerCodes = allocations.Select(a => a.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var shipments = await _apparelProDbContext.PartShipments
                .AsNoTracking()
                .Where(p => buyerCodes.Contains(p.BuyerCode))
                .ToListAsync();

            var lines = allocations.Select(a =>
            {
                var shipment = shipments.FirstOrDefault(p =>
                    p.BuyerCode == a.BuyerCode && p.Order == a.Order && p.TypeCode == a.TypeCode &&
                    p.StyleCode == a.StyleCode && p.NewOrder == a.ShipmentOrder);
                var shipDate = shipment != null ? DateOnly.FromDateTime(shipment.ShipDate) : (DateOnly?)null;

                return new ProductionScheduleLineServiceModel
                {
                    LineCode = a.LineCode,
                    EstimatedStartDate = a.EstimatedStartDate,
                    EstimatedEndDate = a.EstimatedEndDate,
                    BuyerCode = a.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(a.BuyerCode, a.BuyerCode.ToString()),
                    Order = a.Order,
                    TypeCode = a.TypeCode,
                    StyleCode = a.StyleCode,
                    ShipmentOrder = a.ShipmentOrder,
                    EstimatedProductionPerDay = a.EstimatedProductionPerDay,
                    Unit = a.Unit,
                    LeadTimeDays = a.LeadTimeDays,
                    NumberOfDays = a.NumberOfDays,
                    TotalQuantity = a.TotalQuantity,
                    ShipDate = shipDate,
                    FloatDays = shipDate.HasValue ? shipDate.Value.DayNumber - a.EstimatedEndDate.DayNumber : (int?)null
                };
            }).ToList();

            return new ProductionScheduleReportServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Lines = lines
            };
        }
    }
}
