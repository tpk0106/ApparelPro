using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryDailyReportService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_DPROD.PRG's "DAILY PRODUCTION SUMMARY" report - see
    // ProductionSummaryDailyReportServiceModel for the exact column/total
    // semantics (including the legacy totals-row Balance quirk preserved
    // deliberately).
    public class ProductionSummaryDailyReportService : IProductionSummaryDailyReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IUnitConversionService _unitConversionService;

        public ProductionSummaryDailyReportService(
            ApparelProDbContext apparelProDbContext, IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _unitConversionService = unitConversionService;
        }

        public async Task<ProductionSummaryDailyReportServiceModel> GetProductionSummaryDailyReportAsync(DateOnly date)
        {
            var sections = await _apparelProDbContext.Sections
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .ToListAsync();

            var todaysEntries = await _apparelProDbContext.DailyProductionEntries
                .AsNoTracking()
                .Where(e => e.Date == date)
                .ToListAsync();

            // Mirrors legacy's "No entries for Given date" error box.
            if (todaysEntries.Count == 0)
                throw new InvalidOperationException($"No entries for given date {date:dd/MM/yyyy}.");

            var lineKeys = todaysEntries
                .Select(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.LineCode })
                .Distinct()
                .OrderBy(k => k.BuyerCode).ThenBy(k => k.Order).ThenBy(k => k.TypeCode)
                .ThenBy(k => k.StyleCode).ThenBy(k => k.LineCode)
                .ToList();

            // One correlated query per active line (bounded to however many
            // lines actually posted on this date, not the whole table) -
            // same "N subqueries is fine here" tradeoff already accepted in
            // StockMovementReportService for the same reason.
            var toDateByLineAndSection = new Dictionary<(int BuyerCode, string Order, int TypeCode, string StyleCode, string LineCode, string SectionCode), decimal>();
            foreach (var key in lineKeys)
            {
                var sums = await _apparelProDbContext.DailyProductionEntries
                    .AsNoTracking()
                    .Where(e => e.Date <= date && e.BuyerCode == key.BuyerCode && e.Order == key.Order &&
                                e.TypeCode == key.TypeCode && e.StyleCode == key.StyleCode && e.LineCode == key.LineCode)
                    .GroupBy(e => e.SectionCode)
                    .Select(g => new { SectionCode = g.Key, Total = g.Sum(e => e.Quantity) })
                    .ToListAsync();

                foreach (var s in sums)
                {
                    toDateByLineAndSection[(key.BuyerCode, key.Order, key.TypeCode, key.StyleCode, key.LineCode, s.SectionCode)] = s.Total;
                }
            }

            var styleUnits = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => lineKeys.Select(k => k.BuyerCode).Contains(s.BuyerCode) &&
                            lineKeys.Select(k => k.Order).Contains(s.Order))
                .ToDictionaryAsync(s => (s.BuyerCode, s.Order, s.TypeCode, s.StyleCode), s => s.Unit ?? "");

            var descriptions = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .Where(p => lineKeys.Select(k => k.BuyerCode).Contains(p.BuyerCode) &&
                            lineKeys.Select(k => k.Order).Contains(p.Order))
                .ToDictionaryAsync(p => (p.BuyerCode, p.Order), p => p.Description);

            var buyerCodes = lineKeys.Select(k => k.BuyerCode).Distinct().ToList();
            var buyerNames = await _apparelProDbContext.Buyers
                .AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode))
                .ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            var allocations = await _apparelProDbContext.ProductionLineAllocations
                .AsNoTracking()
                .Where(a => lineKeys.Select(k => k.BuyerCode).Contains(a.BuyerCode) &&
                            lineKeys.Select(k => k.Order).Contains(a.Order))
                .ToListAsync();

            var lines = new List<ProductionSummaryDailyLineServiceModel>();
            foreach (var key in lineKeys)
            {
                var styleUnit = styleUnits.GetValueOrDefault((key.BuyerCode, key.Order, key.TypeCode, key.StyleCode), "");

                var lineAllocations = allocations.Where(a =>
                    a.BuyerCode == key.BuyerCode && a.Order == key.Order &&
                    a.TypeCode == key.TypeCode && a.StyleCode == key.StyleCode && a.LineCode == key.LineCode);

                decimal orderQuantity = 0;
                foreach (var allocation in lineAllocations)
                {
                    orderQuantity += await ConvertQuantityAsync(allocation.Unit, styleUnit, allocation.TotalQuantity);
                }

                var sectionTotals = new List<ProductionSummaryDailySectionTotalServiceModel>();
                foreach (var section in sections)
                {
                    var toDate = toDateByLineAndSection.GetValueOrDefault(
                        (key.BuyerCode, key.Order, key.TypeCode, key.StyleCode, key.LineCode, section.Code), 0);

                    var proQtyRaw = todaysEntries
                        .Where(e => e.BuyerCode == key.BuyerCode && e.Order == key.Order && e.TypeCode == key.TypeCode &&
                                    e.StyleCode == key.StyleCode && e.LineCode == key.LineCode && e.SectionCode == section.Code)
                        .Sum(e => (decimal?)e.Quantity) ?? 0;

                    // Entry rows are recorded in the section's own Unit
                    // (DailyProductionEntry.Unit), converted here to the
                    // style's unit the same way the legacy report's
                    // CONVERT() calls did.
                    var entryUnit = todaysEntries
                        .FirstOrDefault(e => e.BuyerCode == key.BuyerCode && e.Order == key.Order && e.TypeCode == key.TypeCode &&
                                              e.StyleCode == key.StyleCode && e.LineCode == key.LineCode && e.SectionCode == section.Code)
                        ?.Unit ?? styleUnit;

                    var proQty = await ConvertQuantityAsync(entryUnit, styleUnit, proQtyRaw);
                    var toDateConverted = await ConvertQuantityAsync(entryUnit, styleUnit, toDate);

                    sectionTotals.Add(new ProductionSummaryDailySectionTotalServiceModel
                    {
                        SectionCode = section.Code,
                        ProQuantity = proQty,
                        ToDateQuantity = toDateConverted,
                        Balance = orderQuantity - toDateConverted
                    });
                }

                lines.Add(new ProductionSummaryDailyLineServiceModel
                {
                    BuyerCode = key.BuyerCode,
                    BuyerName = buyerNames.GetValueOrDefault(key.BuyerCode, key.BuyerCode.ToString()),
                    Order = key.Order,
                    TypeCode = key.TypeCode,
                    StyleCode = key.StyleCode,
                    Description = descriptions.GetValueOrDefault((key.BuyerCode, key.Order)),
                    Unit = styleUnit,
                    LineCode = key.LineCode,
                    OrderQuantity = orderQuantity,
                    Sections = sectionTotals
                });
            }

            var totalOrderQuantity = lines.Sum(l => l.OrderQuantity);
            var totals = sections.Select(section =>
            {
                var proTotal = lines.Sum(l => l.Sections.First(s => s.SectionCode == section.Code).ProQuantity);
                var toDateTotal = lines.Sum(l => l.Sections.First(s => s.SectionCode == section.Code).ToDateQuantity);
                return new ProductionSummaryDailySectionTotalServiceModel
                {
                    SectionCode = section.Code,
                    ProQuantity = proTotal,
                    ToDateQuantity = toDateTotal,
                    // Legacy quirk preserved (see service model comment):
                    // grand-total Order Qty minus grand-total To-Date Qty,
                    // not a sum of the individual line balances.
                    Balance = totalOrderQuantity - toDateTotal
                };
            }).ToList();

            return new ProductionSummaryDailyReportServiceModel
            {
                Date = date,
                SectionCodes = sections.Select(s => s.Code).ToList(),
                SectionDescriptions = sections.Select(s => s.Description).ToList(),
                Lines = lines,
                TotalOrderQuantity = totalOrderQuantity,
                Totals = totals
            };
        }

        private async Task<decimal> ConvertQuantityAsync(string fromUnit, string toUnit, decimal quantity)
        {
            if (string.IsNullOrEmpty(fromUnit) || string.IsNullOrEmpty(toUnit) ||
                string.Equals(fromUnit, toUnit, StringComparison.OrdinalIgnoreCase))
            {
                return quantity;
            }

            var conversion = await _unitConversionService.GetUnitConversionByFromUnitAndToUnitAsync(fromUnit, toUnit);
            return conversion?.Measure is decimal measure ? quantity * measure : quantity;
        }
    }
}
