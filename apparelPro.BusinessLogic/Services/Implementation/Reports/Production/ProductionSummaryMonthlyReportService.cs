using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_MPROD.PRG's "MONTHLY PRODUCTION SUMMARY" report - see
    // ProductionSummaryMonthlyReportServiceModel for the exact semantics
    // (dynamic sub-row stacking, per-Line-not-per-style cumulative).
    public class ProductionSummaryMonthlyReportService : IProductionSummaryMonthlyReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionSummaryMonthlyReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        private record RawEntry(
            DateOnly Date, string LineCode, int BuyerCode, string Order, int TypeCode, string StyleCode,
            decimal? EstQuantity, decimal? ActQuantity);

        public async Task<ProductionSummaryMonthlyReportServiceModel> GetReportAsync(int year, int month)
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

            var days = new List<ProductionSummaryMonthlyDayRowServiceModel>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var entriesByLine = lines.ToDictionary(
                    l => l.LineCode, l => byDateLine.GetValueOrDefault((date, l.LineCode)) ?? new List<RawEntry>());
                var hasAnyEntry = entriesByLine.Values.Any(v => v.Count > 0);

                if (!hasAnyEntry && holidays.TryGetValue(date, out var holidayDesc))
                {
                    days.Add(new ProductionSummaryMonthlyDayRowServiceModel
                    {
                        Date = date,
                        IsHoliday = true,
                        HolidayDescription = holidayDesc,
                        SubRows = new()
                    });
                    continue;
                }

                // Legacy always executes at least one pass per non-holiday
                // day (fnd1/fnd2 start true), so a data-less day still gets
                // one (blank) row rather than being skipped.
                var passCount = Math.Max(entriesByLine.Values.Max(v => v.Count), 1);
                var subRows = new List<ProductionSummaryMonthlySubRowServiceModel>();

                for (var pass = 0; pass < passCount; pass++)
                {
                    var cells = new List<ProductionSummaryMonthlyCellServiceModel>();
                    decimal passEst = 0, passAct = 0;

                    foreach (var line in lines)
                    {
                        var entries = entriesByLine[line.LineCode];
                        var entry = pass < entries.Count ? entries[pass] : null;

                        if (entry?.EstQuantity is decimal est) runningEst[line.LineCode] += est;
                        if (entry?.ActQuantity is decimal act) runningAct[line.LineCode] += act;

                        cells.Add(new ProductionSummaryMonthlyCellServiceModel
                        {
                            StyleCode = entry?.StyleCode,
                            EstQuantity = entry?.EstQuantity,
                            ActQuantity = entry?.ActQuantity,
                            // Always shown, regardless of whether this line
                            // had an entry this pass - matches legacy
                            // printing estcum/actcum unconditionally.
                            CumEstQuantity = runningEst[line.LineCode],
                            CumActQuantity = runningAct[line.LineCode]
                        });

                        passEst += entry?.EstQuantity ?? 0;
                        passAct += entry?.ActQuantity ?? 0;
                    }

                    subRows.Add(new ProductionSummaryMonthlySubRowServiceModel
                    {
                        LineCells = cells,
                        TotalEstQuantity = passEst,
                        TotalActQuantity = passAct,
                        TotalCumEstQuantity = runningEst.Values.Sum(),
                        TotalCumActQuantity = runningAct.Values.Sum()
                    });
                }

                days.Add(new ProductionSummaryMonthlyDayRowServiceModel
                {
                    Date = date,
                    IsHoliday = false,
                    SubRows = subRows
                });
            }

            return new ProductionSummaryMonthlyReportServiceModel
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
                .ToListAsync();

            var actuals = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date >= startDate && e.Date <= endDate && e.SectionCode == finalSectionCode)
                .ToListAsync();

            var keys = estimates
                .Select(e => (e.Date, e.LineCode, e.BuyerCode, e.Order, e.TypeCode, e.StyleCode))
                .Union(actuals.Select(a => (a.Date, a.LineCode, a.BuyerCode, a.Order, a.TypeCode, a.StyleCode)));

            return keys.Select(k => new RawEntry(
                k.Date, k.LineCode, k.BuyerCode, k.Order, k.TypeCode, k.StyleCode,
                estimates.FirstOrDefault(e => e.Date == k.Date && e.LineCode == k.LineCode && e.BuyerCode == k.BuyerCode &&
                                               e.Order == k.Order && e.TypeCode == k.TypeCode && e.StyleCode == k.StyleCode)?.Quantity,
                actuals.FirstOrDefault(a => a.Date == k.Date && a.LineCode == k.LineCode && a.BuyerCode == k.BuyerCode &&
                                             a.Order == k.Order && a.TypeCode == k.TypeCode && a.StyleCode == k.StyleCode)?.Quantity
            )).ToList();
        }
    }
}
