using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionAnalysisSummaryReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_MPRO2.PRG's "PRODUCTION ANALYSIS SUMMARY - (For Style)"
    // report - see ProductionAnalysisSummaryReportServiceModel for the exact
    // column semantics and the deliberate column-alignment fix.
    public class ProductionAnalysisSummaryReportService : IProductionAnalysisSummaryReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionAnalysisSummaryReportService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ProductionAnalysisSummaryReportServiceModel> GetReportAsync(
            int buyerCode, string order, int typeCode, string styleCode)
        {
            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            // Mirrors legacy's "Invalid Buyer code" error box.
            if (buyerName == null)
                throw new InvalidOperationException("Invalid Buyer code.");

            var typeName = await _apparelProDbContext.GarmentTypes
                .AsNoTracking()
                .Where(t => t.Id == typeCode)
                .Select(t => t.TypeName)
                .FirstOrDefaultAsync() ?? typeCode.ToString();

            var sections = await _apparelProDbContext.Sections.AsNoTracking().OrderBy(s => s.Code).ToListAsync();
            var finalSection = sections.FirstOrDefault(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var entries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order && e.TypeCode == typeCode && e.StyleCode == styleCode)
                .ToListAsync();

            // Mirrors legacy's "Buyer+order+type+style not in the file" error box.
            if (entries.Count == 0)
                throw new InvalidOperationException("Buyer+order+type+style not in the file.");

            var rows = new List<ProductionAnalysisRowServiceModel>();
            var sectionTotals = sections.ToDictionary(s => s.Code, _ => 0m);
            var finalOutputProductionDays = 0;
            var totalDaysTakenForProduction = 0;

            foreach (var group in entries.GroupBy(e => new { e.Date, e.LineCode }).OrderBy(g => g.Key.Date).ThenBy(g => g.Key.LineCode))
            {
                var sectionQuantities = new List<ProductionAnalysisSectionQtyServiceModel>();
                decimal rowTotal = 0;

                foreach (var section in sections)
                {
                    var qty = group.Where(e => e.SectionCode == section.Code).Sum(e => e.Quantity);
                    sectionQuantities.Add(new ProductionAnalysisSectionQtyServiceModel { SectionCode = section.Code, Quantity = qty });
                    sectionTotals[section.Code] += qty;
                    rowTotal += qty;
                }

                var finalSectionQty = sectionQuantities.First(s => s.SectionCode == finalSection.Code).Quantity;
                if (finalSectionQty != 0)
                    finalOutputProductionDays++;
                if (rowTotal != 0)
                    totalDaysTakenForProduction++;

                rows.Add(new ProductionAnalysisRowServiceModel
                {
                    Date = group.Key.Date,
                    LineCode = group.Key.LineCode,
                    SectionQuantities = sectionQuantities,
                    Total = rowTotal
                });
            }

            var totalFinalSectionQty = sectionTotals.GetValueOrDefault(finalSection.Code, 0m);
            var averageProductionQuantityOnFinalOutput = finalOutputProductionDays == 0
                ? 0
                : totalFinalSectionQty / finalOutputProductionDays;

            return new ProductionAnalysisSummaryReportServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                TypeName = typeName,
                StyleCode = styleCode,
                SectionCodes = sections.Select(s => s.Code).ToList(),
                SectionDescriptions = sections.Select(s => s.Description).ToList(),
                FinalSectionCode = finalSection.Code,
                FinalSectionDescription = finalSection.Description,
                Rows = rows,
                SectionTotals = sections.Select(s => new ProductionAnalysisSectionQtyServiceModel
                {
                    SectionCode = s.Code,
                    Quantity = sectionTotals[s.Code]
                }).ToList(),
                AverageProductionQuantityOnFinalOutput = averageProductionQuantityOnFinalOutput,
                FinalOutputProductionDays = finalOutputProductionDays,
                TotalDaysTakenForProduction = totalDaysTakenForProduction
            };
        }
    }
}
