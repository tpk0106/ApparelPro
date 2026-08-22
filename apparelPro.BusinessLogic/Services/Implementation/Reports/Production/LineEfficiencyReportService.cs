using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.ILineEfficiencyReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_GRH1.PRG's "LINE EFFICIENCY" report - see
    // LineEfficiencyReportServiceModel for the exact formula and the
    // deliberate departure from the legacy loop-overwrite quirk.
    public class LineEfficiencyReportService : ILineEfficiencyReportService
    {
        private const string WorkHoursPerDayKey = "ProductionWorkHoursPerDay";

        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ISystemParameterService _systemParameterService;

        public LineEfficiencyReportService(
            ApparelProDbContext apparelProDbContext, ISystemParameterService systemParameterService)
        {
            _apparelProDbContext = apparelProDbContext;
            _systemParameterService = systemParameterService;
        }

        public async Task<LineEfficiencyReportServiceModel> GetReportAsync(string lineCode, int year, int month)
        {
            if (month is < 1 or > 12 || year is < 1 or > 9999)
                throw new InvalidOperationException("Invalid Month/Year.");

            var line = await _apparelProDbContext.ProductionLines
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LineCode == lineCode);

            // Mirrors legacy's "Invalid Line Code." error box.
            if (line == null)
                throw new InvalidOperationException("Invalid Line Code.");

            var finalSection = await _apparelProDbContext.Sections.AsNoTracking().FirstOrDefaultAsync(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var startDate = new DateOnly(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var endDate = startDate.AddDays(daysInMonth - 1);

            var entries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.LineCode == lineCode && e.SectionCode == finalSection.Code &&
                            e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Mirrors legacy's "No data available for given Line and Month." error box.
            if (entries.Count == 0)
                throw new InvalidOperationException("No data available for given Line and Month.");

            var styleKeys = entries
                .Select(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .Distinct()
                .ToList();
            var buyerCodes = styleKeys.Select(k => k.BuyerCode).Distinct().ToList();
            var orders = styleKeys.Select(k => k.Order).Distinct().ToList();
            var styleKeySet = styleKeys.Select(k => (k.BuyerCode, k.Order, k.TypeCode, k.StyleCode)).ToHashSet();

            var allBreakdown = await _apparelProDbContext.StyleOperationBreakdowns
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode) && orders.Contains(b.Order))
                .ToListAsync();
            var relevantBreakdown = allBreakdown
                .Where(b => styleKeySet.Contains((b.BuyerCode, b.Order, b.TypeCode, b.StyleCode)))
                .ToList();

            var machineTypeCodes = relevantBreakdown.Select(b => b.MachineTypeCode).Distinct().ToList();
            var manualMachineTypes = await _apparelProDbContext.MachineTypes
                .AsNoTracking()
                .Where(m => machineTypeCodes.Contains(m.Code) && m.IsManual)
                .Select(m => m.Code)
                .ToListAsync();
            var manualMachineTypeSet = manualMachineTypes.ToHashSet();

            var stdHoursPerUnitByStyle = relevantBreakdown
                .Where(b => !manualMachineTypeSet.Contains(b.MachineTypeCode))
                .GroupBy(b => (b.BuyerCode, b.Order, b.TypeCode, b.StyleCode))
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(b => b.Sam) / 60m, 2));

            var allAllocations = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => a.LineCode == lineCode && buyerCodes.Contains(a.BuyerCode) && orders.Contains(a.Order))
                .ToListAsync();
            var machineCountByStyle = allAllocations
                .Where(a => styleKeySet.Contains((a.BuyerCode, a.Order, a.TypeCode, a.StyleCode)))
                .GroupBy(a => (a.BuyerCode, a.Order, a.TypeCode, a.StyleCode))
                .ToDictionary(g => g.Key, g => (decimal)g.Sum(a => a.NumberOfMachines));

            var workHoursPerDay = await _systemParameterService.GetDecimalValueAsync(WorkHoursPerDayKey, 8m);

            var holidays = await _apparelProDbContext.Holidays
                .AsNoTracking()
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .ToDictionaryAsync(h => h.Date, h => h.Description);

            var entriesByDate = entries.GroupBy(e => e.Date).ToDictionary(g => g.Key, g => g.ToList());

            var days = new List<LineEfficiencyDayCellServiceModel>();
            decimal totalPercent = 0;
            var daysWithProduction = 0;

            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);
                var isHoliday = holidays.TryGetValue(date, out var holidayDescription);

                if (!entriesByDate.TryGetValue(date, out var dayEntries))
                {
                    days.Add(new LineEfficiencyDayCellServiceModel
                    {
                        Day = day,
                        DayOfWeek = date.DayOfWeek.ToString(),
                        IsHoliday = isHoliday,
                        HolidayDescription = holidayDescription,
                        EfficiencyPercent = null
                    });
                    continue;
                }

                decimal earnedHours = 0;
                decimal machineHours = 0;

                foreach (var styleGroup in dayEntries.GroupBy(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode }))
                {
                    var key = (styleGroup.Key.BuyerCode, styleGroup.Key.Order, styleGroup.Key.TypeCode, styleGroup.Key.StyleCode);
                    var actualQty = styleGroup.Sum(e => e.Quantity);
                    var stdHoursPerUnit = stdHoursPerUnitByStyle.GetValueOrDefault(key, 0);
                    var machineCount = machineCountByStyle.GetValueOrDefault(key, line.NumberOfMachines);

                    earnedHours += actualQty * stdHoursPerUnit;
                    machineHours += machineCount * workHoursPerDay;
                }

                var dayPercent = machineHours == 0 ? 0 : Math.Round(earnedHours / machineHours * 100m, 0);

                days.Add(new LineEfficiencyDayCellServiceModel
                {
                    Day = day,
                    DayOfWeek = date.DayOfWeek.ToString(),
                    IsHoliday = isHoliday,
                    HolidayDescription = holidayDescription,
                    EfficiencyPercent = dayPercent
                });

                totalPercent += dayPercent;
                daysWithProduction++;
            }

            return new LineEfficiencyReportServiceModel
            {
                LineCode = line.LineCode,
                LineDescription = line.Description,
                Year = year,
                Month = month,
                DaysInMonth = daysInMonth,
                FinalSectionCode = finalSection.Code,
                FinalSectionDescription = finalSection.Description,
                WorkHoursPerDay = workHoursPerDay,
                Days = days,
                MonthlyAverageEfficiencyPercent = daysWithProduction == 0 ? 0 : totalPercent / daysWithProduction
            };
        }
    }
}
