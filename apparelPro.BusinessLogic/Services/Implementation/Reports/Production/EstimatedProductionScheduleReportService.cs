using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionScheduleReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_ESTL2.PRG's "ESTIMATED PRODUCTION SCHEDULE" report.
    public class EstimatedProductionScheduleReportService : IEstimatedProductionScheduleReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public EstimatedProductionScheduleReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<EstimatedProductionScheduleReportServiceModel> GetReportAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Mirrors legacy's own guard ("From Date cannot be greater than To Date.").
            if (fromDate > toDate)
                throw new InvalidOperationException("From Date cannot be greater than To Date.");

            var allocations = await _apparelProDbContext.EstimatedProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.EstimatedStartDate >= fromDate && a.EstimatedStartDate <= toDate)
                .OrderBy(a => a.LineCode).ThenBy(a => a.EstimatedStartDate)
                .ThenBy(a => a.BuyerCode).ThenBy(a => a.StyleCode)
                .ToListAsync();

            // Mirrors legacy's "Given date Range not found." error box.
            if (allocations.Count == 0)
                throw new InvalidOperationException("Given date range not found.");

            var buyerCodes = allocations.Select(a => a.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var rows = allocations.Select(a => new EstimatedProductionScheduleRowServiceModel
            {
                LineCode = a.LineCode,
                EstStartDate = a.EstimatedStartDate,
                EstEndDate = a.EstimatedEndDate,
                BuyerCode = a.BuyerCode,
                BuyerName = buyerNames.GetValueOrDefault(a.BuyerCode, a.BuyerCode.ToString()),
                StyleCode = a.StyleCode,
                EstimatedProductionPerDay = a.EstimatedProductionPerDay,
                Unit = a.Unit,
                LeadTimeDays = a.LeadTimeDays,
                NumberOfDays = a.NumberOfDays,
                TotalQuantity = a.TotalQuantity,
                ShipDate = a.ShipDate,
                FloatDays = a.ShipDate.DayNumber - a.EstimatedEndDate.DayNumber
            }).ToList();

            return new EstimatedProductionScheduleReportServiceModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows
            };
        }
    }
}
