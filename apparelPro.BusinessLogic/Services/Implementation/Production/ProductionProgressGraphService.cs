using apparelPro.BusinessLogic.Services.Models.Production.IProductionProgressGraphService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Production
{
    // Replicates PR_PROG.PRG's "PRODUCTION PROGRESS" graph - see
    // ProductionProgressGraphServiceModel for the exact series semantics.
    public class ProductionProgressGraphService : IProductionProgressGraphService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public ProductionProgressGraphService(ApparelProDbContext apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<ProductionProgressGraphServiceModel> GetGraphAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var buyerName = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => b.BuyerCode == buyerCode)
                .Select(b => b.Name)
                .FirstOrDefaultAsync();

            var styleExists = await _apparelProDbContext.Styles
                .AsNoTracking()
                .AnyAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);

            // Mirrors legacy's "Invalid Buyer/Order/Type/Style" error box.
            if (buyerName == null || !styleExists)
                throw new InvalidOperationException("Invalid Buyer/Order/Type/Style.");

            var finalSection = await _apparelProDbContext.Sections.AsNoTracking().FirstOrDefaultAsync(s => s.IsFinal)
                ?? throw new InvalidOperationException("No Section is marked as Final.");

            var estimatedEntries = await _apparelProDbContext.EstimatedProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order && e.TypeCode == typeCode && e.StyleCode == styleCode)
                .ToListAsync();

            // Mirrors legacy's "No estimates entered for given Order/Style" error box.
            if (estimatedEntries.Count == 0)
                throw new InvalidOperationException("No estimates entered for given Order/Style.");

            var actualEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.BuyerCode == buyerCode && e.Order == order && e.TypeCode == typeCode &&
                            e.StyleCode == styleCode && e.SectionCode == finalSection.Code)
                .ToListAsync();

            return new ProductionProgressGraphServiceModel
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                FinalSectionCode = finalSection.Code,
                FinalSectionDescription = finalSection.Description,
                EstimatedSeries = BuildCumulativeSeries(estimatedEntries.Select(e => (e.Date, e.Quantity))),
                ActualSeries = BuildCumulativeSeries(actualEntries.Select(e => (e.Date, e.Quantity)))
            };
        }

        private static List<ProductionProgressPointServiceModel> BuildCumulativeSeries(IEnumerable<(DateOnly Date, decimal Quantity)> entries)
        {
            var byDate = entries
                .GroupBy(e => e.Date)
                .Select(g => new { Date = g.Key, Quantity = g.Sum(e => e.Quantity) })
                .OrderBy(g => g.Date)
                .ToList();

            var points = new List<ProductionProgressPointServiceModel>();
            decimal cumulative = 0;
            var day = 1;
            foreach (var entry in byDate)
            {
                cumulative += entry.Quantity;
                points.Add(new ProductionProgressPointServiceModel { DayNumber = day, CumulativeQuantity = cumulative });
                day++;
            }

            return points;
        }
    }
}
