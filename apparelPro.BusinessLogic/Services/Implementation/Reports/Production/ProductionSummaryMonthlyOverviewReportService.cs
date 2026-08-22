using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyOverviewReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Simplified companion to ProductionSummaryMonthlyReportService - see
    // ProductionSummaryMonthlyOverviewReportServiceModel for how it differs
    // (one row per line/day, no dynamic sub-row stacking).
    public class ProductionSummaryMonthlyOverviewReportService : IProductionSummaryMonthlyOverviewReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionSummaryMonthlyOverviewReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private record RawEntry(DateOnly Date, string LineCode, decimal? EstQuantity, decimal? ActQuantity);

        public async Task<ProductionSummaryMonthlyOverviewReportServiceModel> GetReportAsync(int year, int month)
        {
            // Mirrors legacy's own "Invalid Month/Year" error box - a bad
            // year/month used to throw an unhandled DateOnly exception
            // (500) instead of a friendly message.
            if (month is < 1 or > 12 || year is < 1 or > 9999)
                throw new InvalidOperationException("Invalid Month/Year.");

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var finalSection = await _apparelProDbContext.Sections.AsNoTracking().FirstOrDefaultAsync(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var lines = await _apparelProDbContext.ProductionLines.AsNoTracking().OrderBy(l => l.LineCode).ToListAsync();
            var rawEntries = await GetRawEntriesAsync(startDate, endDate, finalSection.Code);
            var holidays = await _apparelProDbContext.Holidays
                .AsNoTracking()
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .ToDictionaryAsync(h => h.Date, h => h.Description);

            var byDateLine = rawEntries
                .GroupBy(r => (r.Date, r.LineCode))
                .ToDictionary(g => g.Key, g => g.ToList());

            var runningEst = lines.ToDictionary(l => l.LineCode, _ => 0m);
            var runningAct = lines.ToDictionary(l => l.LineCode, _ => 0m);

            var days = new List<ProductionSummaryMonthlyOverviewDayRowServiceModel>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var hasAnyEntry = lines.Any(l => byDateLine.ContainsKey((date, l.LineCode)));

                if (!hasAnyEntry && holidays.TryGetValue(date, out var holidayDesc))
                {
                    days.Add(new ProductionSummaryMonthlyOverviewDayRowServiceModel
                    {
                        Date = date,
                        IsHoliday = true,
                        HolidayDescription = holidayDesc,
                        LineCells = new()
                    });
                    continue;
                }

                var cells = new List<ProductionSummaryMonthlyOverviewCellServiceModel>();
                decimal dayTotalEst = 0, dayTotalAct = 0;

                foreach (var line in lines)
                {
                    var entries = byDateLine.GetValueOrDefault((date, line.LineCode)) ?? new List<RawEntry>();
                    var dayEst = entries.Sum(e => e.EstQuantity ?? 0);
                    var dayAct = entries.Sum(e => e.ActQuantity ?? 0);
                    runningEst[line.LineCode] += dayEst;
                    runningAct[line.LineCode] += dayAct;

                    cells.Add(new ProductionSummaryMonthlyOverviewCellServiceModel
                    {
                        EstQuantity = dayEst,
                        ActQuantity = dayAct,
                        CumEstQuantity = runningEst[line.LineCode],
                        CumActQuantity = runningAct[line.LineCode]
                    });

                    dayTotalEst += dayEst;
                    dayTotalAct += dayAct;
                }

                days.Add(new ProductionSummaryMonthlyOverviewDayRowServiceModel
                {
                    Date = date,
                    IsHoliday = false,
                    LineCells = cells,
                    TotalEstQuantity = dayTotalEst,
                    TotalActQuantity = dayTotalAct,
                    TotalCumEstQuantity = runningEst.Values.Sum(),
                    TotalCumActQuantity = runningAct.Values.Sum()
                });
            }

            return new ProductionSummaryMonthlyOverviewReportServiceModel
            {
                Year = year,
                Month = month,
                FinalSectionCode = finalSection.Code,
                FinalSectionDescription = finalSection.Description,
                LineCodes = lines.Select(l => l.LineCode).ToList(),
                LineDescriptions = lines.Select(l => l.Description).ToList(),
                Days = days
            };
        }

        private async Task<List<RawEntry>> GetRawEntriesAsync(DateOnly startDate, DateOnly endDate, string finalSectionCode)
        {
            var estimates = await _apparelProDbContext.EstimatedProductionEntries
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .Select(e => new { e.Date, e.LineCode, e.Quantity })
                .ToListAsync();

            var actuals = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate && e.SectionCode == finalSectionCode)
                .Select(e => new { e.Date, e.LineCode, e.Quantity })
                .ToListAsync();

            // Each estimate/actual row becomes its own RawEntry (not merged
            // by date+line up front) so a line with multiple styles' entries
            // the same day still contributes each one to the day's sum,
            // matching the Detailed report's same raw data.
            var entries = estimates
                .Select(e => new RawEntry(e.Date, e.LineCode, e.Quantity, null))
                .Concat(actuals.Select(a => new RawEntry(a.Date, a.LineCode, null, a.Quantity)))
                .ToList();

            return entries;
        }
    }
}
