using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.ILineProductionSummaryReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_LPROD.PRG's "LINE PRODUCTION SUMMARY" report - see
    // LineProductionSummaryReportServiceModel for the exact Cumulative
    // column semantics (deliberately not bounded by Start Date).
    public class LineProductionSummaryReportService : ILineProductionSummaryReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public LineProductionSummaryReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<LineProductionSummaryReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
                throw new InvalidOperationException("Start Date must not be after End Date.");

            var finalSection = await _apparelProDbContext.Sections.AsNoTracking().FirstOrDefaultAsync(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var lines = await _apparelProDbContext.ProductionLines
                .AsNoTracking()
                .OrderBy(l => l.LineCode)
                .ToListAsync();

            // Only the Final section matters for this report, and only up to
            // EndDate (the Cumulative column's own upper bound) - so a single
            // query covers both PeriodQty and CumulativeQty per line.
            var entries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.SectionCode == finalSection.Code && e.Date <= endDate)
                .Select(e => new { e.LineCode, e.Date, e.Quantity })
                .ToListAsync();

            var rows = new List<LineProductionSummaryRowServiceModel>();
            foreach (var line in lines)
            {
                var lineEntries = entries.Where(e => e.LineCode == line.LineCode);

                var periodQty = lineEntries
                    .Where(e => e.Date >= startDate && e.Date <= endDate)
                    .Sum(e => (decimal?)e.Quantity) ?? 0;

                var cumulativeQty = lineEntries.Sum(e => (decimal?)e.Quantity) ?? 0;

                rows.Add(new LineProductionSummaryRowServiceModel
                {
                    LineCode = line.LineCode,
                    LineDescription = line.Description,
                    PeriodQty = periodQty,
                    CumulativeQty = cumulativeQty
                });
            }

            return new LineProductionSummaryReportServiceModel
            {
                StartDate = startDate,
                EndDate = endDate,
                FinalSectionCode = finalSection.Code,
                FinalSectionDescription = finalSection.Description,
                Rows = rows,
                TotalPeriodQty = rows.Sum(r => r.PeriodQty),
                TotalCumulativeQty = rows.Sum(r => r.CumulativeQty)
            };
        }
    }
}
