using apparelPro.BusinessLogic.Production;
using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IDailyEmployeeEfficiencyReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_EEF1.PRG's "DAILY EMPLOYEE EFFICIENCY" report - see
    // DailyEmployeeEfficiencyReportServiceModel for how it reuses the
    // already-ported DailyProductionEfficiencyCalculator.
    public class DailyEmployeeEfficiencyReportService : IDailyEmployeeEfficiencyReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public DailyEmployeeEfficiencyReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<DailyEmployeeEfficiencyReportServiceModel> GetReportAsync(DateOnly date, string? lineCode)
        {
            string? lineDescription = null;
            if (!string.IsNullOrEmpty(lineCode))
            {
                lineDescription = await _apparelProDbContext.ProductionLines
                    .AsNoTracking()
                    .Where(l => l.LineCode == lineCode)
                    .Select(l => l.Description)
                    .FirstOrDefaultAsync();

                // Mirrors legacy's "Invalid Line No." error box.
                if (lineDescription == null)
                    throw new InvalidOperationException("Invalid Line No.");
            }

            var entries = await _apparelProDbContext.DailyProductionTimeTicketEntries
                .AsNoTracking()
                .Where(e => e.Date == date && (lineCode == null || e.LineCode == lineCode))
                .ToListAsync();

            // Mirrors legacy's "No details available for printing" error box.
            if (entries.Count == 0)
                throw new InvalidOperationException("No details available for printing.");

            var styleKeys = entries
                .Select(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .Distinct()
                .ToList();
            var buyerCodes = styleKeys.Select(k => k.BuyerCode).Distinct().ToList();
            var orders = styleKeys.Select(k => k.Order).Distinct().ToList();

            var allBreakdown = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode) && orders.Contains(b.Order))
                .ToListAsync();
            var styleKeySet = styleKeys.Select(k => (k.BuyerCode, k.Order, k.TypeCode, k.StyleCode)).ToHashSet();

            // Same "first match wins" tie-break already used by
            // DailyProductionTimeTicketService for the same ambiguity
            // (an OperationCode can legitimately appear more than once in a
            // style's breakdown, at different components/positions).
            var samByStyleAndOperation = allBreakdown
                .Where(b => styleKeySet.Contains((b.BuyerCode, b.Order, b.TypeCode, b.StyleCode)))
                .GroupBy(b => (b.BuyerCode, b.Order, b.TypeCode, b.StyleCode, b.OperationCode))
                .ToDictionary(g => g.Key, g => g.First().Sam);

            var calculatorInputs = entries.Select(e => new ProductionEntryInput
            {
                EmployeeCode = e.EmployeeCode,
                Quantity = e.Quantity,
                Sam = samByStyleAndOperation.TryGetValue(
                    (e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.OperationCode), out var sam) ? sam : 0,
                NonProductiveHours = e.NonProductiveHours,
                WorkHours = e.WorkHours
            }).ToList();

            var summaries = DailyProductionEfficiencyCalculator.Summarize(calculatorInputs);

            var employeeCodes = summaries.Select(s => s.EmployeeCode).ToList();
            var employeeNames = await _apparelProDbContext.Employees
                .AsNoTracking()
                .Where(e => employeeCodes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, e => e.Name);

            var rows = summaries.Select(s => new EmployeeEfficiencyRowServiceModel
            {
                EmployeeCode = s.EmployeeCode,
                EmployeeName = employeeNames.GetValueOrDefault(s.EmployeeCode, ""),
                WorkHours = s.WorkHours,
                EarnedHours = s.EarnedMinutes / 60m,
                NonProductiveHours = s.NonProductiveHours,
                OverEfficiencyPercent = s.OverEfficiencyPercent,
                OperatorEfficiencyPercent = s.OperatorEfficiencyPercent
            })
            .OrderBy(r => r.EmployeeCode)
            .ToList();

            return new DailyEmployeeEfficiencyReportServiceModel
            {
                Date = date,
                LineCode = lineCode,
                LineDescription = lineDescription,
                Rows = rows
            };
        }
    }
}
