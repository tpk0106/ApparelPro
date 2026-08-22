using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IMonthlyEmployeeEfficiencyReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_REP4.PRG's "EMPLOYEE EFFICIENCY REPORT" - see
    // MonthlyEmployeeEfficiencyReportServiceModel for the exact column
    // semantics and the deliberate WorkHours-source difference from the
    // Daily report.
    public class MonthlyEmployeeEfficiencyReportService : IMonthlyEmployeeEfficiencyReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;

        public MonthlyEmployeeEfficiencyReportService(
            ApparelProDbContext apparelProDbContext, ISystemParameterService systemParameterService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
        }

        public async Task<MonthlyEmployeeEfficiencyReportServiceModel> GetReportAsync(int year, int month)
        {
            if (month is < 1 or > 12 || year is < 1 or > 9999)
                throw new InvalidOperationException("Invalid Month/Year.");

            var startDate = new DateOnly(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var endDate = startDate.AddDays(daysInMonth - 1);

            var entries = await _apparelProDbContext.DailyProductionTimeTicketEntries
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Mirrors legacy's "No Details Present for Printing." error box.
            if (entries.Count == 0)
                throw new InvalidOperationException("No Details Present for Printing.");

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

            // Same "first match wins" tie-break as the Daily report and the
            // existing Daily Production Time Ticket screen.
            var samByStyleAndOperation = allBreakdown
                .Where(b => styleKeySet.Contains((b.BuyerCode, b.Order, b.TypeCode, b.StyleCode)))
                .GroupBy(b => (b.BuyerCode, b.Order, b.TypeCode, b.StyleCode, b.OperationCode))
                .ToDictionary(g => g.Key, g => g.First().Sam);

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);

            var employeeCodes = entries.Select(e => e.EmployeeCode).Distinct().ToList();
            var employeeNames = await _apparelProDbContext.Employees
                .AsNoTracking()
                .Where(e => employeeCodes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, e => e.Name);

            var rows = new List<EmployeeMonthlyEfficiencyRowServiceModel>();
            foreach (var employeeGroup in entries.GroupBy(e => e.EmployeeCode).OrderBy(g => g.Key))
            {
                var days = new List<EmployeeMonthlyEfficiencyDayCellServiceModel>();
                decimal totalPercent = 0;
                var workedDays = 0;

                for (var day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);
                    var dayEntries = employeeGroup.Where(e => e.Date == date).ToList();

                    if (dayEntries.Count == 0)
                    {
                        days.Add(new EmployeeMonthlyEfficiencyDayCellServiceModel { Day = day, OperatorEfficiencyPercent = null });
                        continue;
                    }

                    decimal earnedMinutes = 0;
                    decimal nonProductiveHours = 0;
                    foreach (var entry in dayEntries)
                    {
                        var sam = samByStyleAndOperation.TryGetValue(
                            (entry.BuyerCode, entry.Order, entry.TypeCode, entry.StyleCode, entry.OperationCode), out var s) ? s : 0;
                        earnedMinutes += entry.Quantity * sam;
                        nonProductiveHours += entry.NonProductiveHours;
                    }

                    var availableHours = workHoursPerDay - nonProductiveHours;
                    var operatorEfficiencyPercent = availableHours == 0 ? 0 : earnedMinutes / 60m / availableHours * 100m;

                    days.Add(new EmployeeMonthlyEfficiencyDayCellServiceModel { Day = day, OperatorEfficiencyPercent = operatorEfficiencyPercent });
                    totalPercent += operatorEfficiencyPercent;
                    workedDays++;
                }

                rows.Add(new EmployeeMonthlyEfficiencyRowServiceModel
                {
                    EmployeeCode = employeeGroup.Key,
                    EmployeeName = employeeNames.GetValueOrDefault(employeeGroup.Key, ""),
                    Days = days,
                    MonthlyAverageEfficiencyPercent = workedDays == 0 ? 0 : totalPercent / workedDays
                });
            }

            return new MonthlyEmployeeEfficiencyReportServiceModel
            {
                Year = year,
                Month = month,
                DaysInMonth = daysInMonth,
                WorkHoursPerDay = workHoursPerDay,
                Rows = rows
            };
        }
    }
}
