using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseDetailedReportService;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;
using ApparelPro.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Per-line companion to ProductionSummaryStyleWiseReportService - same
    // date-range/style grouping (shared via ProductionSummaryStyleWiseReportDataLoader),
    // but Order Qty and section quantities are broken down per production
    // Line rather than summed to one total.
    public class ProductionSummaryStyleWiseDetailedReportService : IProductionSummaryStyleWiseDetailedReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IUnitConversionService _unitConversionService;

        public ProductionSummaryStyleWiseDetailedReportService(
            ApparelProDbContext apparelProDbContext, IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _unitConversionService = unitConversionService;
        }

        public async Task<ProductionSummaryStyleWiseDetailedReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate)
        {
            var data = await ProductionSummaryStyleWiseReportDataLoader.LoadAsync(_apparelProDbContext, startDate, endDate);

            var rows = new List<ProductionSummaryStyleWiseDetailedRowServiceModel>();
            foreach (var key in data.StyleKeys)
            {
                var style = data.Styles.GetValueOrDefault(key);
                var styleUnit = style?.Unit ?? "";

                var lineCodes = data.Entries
                    .Where(e => e.BuyerCode == key.BuyerCode && e.Order == key.Order &&
                                e.TypeCode == key.TypeCode && e.StyleCode == key.StyleCode)
                    .Select(e => e.LineCode)
                    .Distinct()
                    .OrderBy(l => l)
                    .ToList();

                var lines = new List<ProductionSummaryStyleWiseDetailedLineServiceModel>();
                decimal finalSectionQtyTotal = 0;
                foreach (var lineCode in lineCodes)
                {
                    var lineAllocations = data.Allocations.Where(a =>
                        a.BuyerCode == key.BuyerCode && a.Order == key.Order && a.TypeCode == key.TypeCode &&
                        a.StyleCode == key.StyleCode && a.LineCode == lineCode);

                    decimal orderQty = 0;
                    foreach (var allocation in lineAllocations)
                    {
                        orderQty += await ConvertQuantityAsync(allocation.Unit, styleUnit, allocation.TotalQuantity);
                    }

                    var sectionQuantities = new List<ProductionSummaryStyleWiseSectionQtyServiceModel>();
                    foreach (var section in data.Sections)
                    {
                        var sectionEntries = data.Entries.Where(e =>
                            e.BuyerCode == key.BuyerCode && e.Order == key.Order && e.TypeCode == key.TypeCode &&
                            e.StyleCode == key.StyleCode && e.LineCode == lineCode && e.SectionCode == section.Code);

                        decimal qty = 0;
                        foreach (var entry in sectionEntries)
                        {
                            qty += await ConvertQuantityAsync(entry.Unit, styleUnit, entry.Quantity);
                        }

                        sectionQuantities.Add(new ProductionSummaryStyleWiseSectionQtyServiceModel
                        {
                            SectionCode = section.Code,
                            Quantity = qty
                        });

                        if (section.Code == data.FinalSection.Code)
                            finalSectionQtyTotal += qty;
                    }

                    lines.Add(new ProductionSummaryStyleWiseDetailedLineServiceModel
                    {
                        LineCode = lineCode,
                        OrderQty = orderQty,
                        SectionQuantities = sectionQuantities
                    });
                }

                var unitPrice = style?.UnitPrice ?? 0;
                rows.Add(new ProductionSummaryStyleWiseDetailedRowServiceModel
                {
                    BuyerCode = key.BuyerCode,
                    BuyerName = data.BuyerNames.GetValueOrDefault(key.BuyerCode, key.BuyerCode.ToString()),
                    Order = key.Order,
                    StyleCode = key.StyleCode,
                    Description = data.Descriptions.GetValueOrDefault((key.BuyerCode, key.Order)),
                    Unit = styleUnit,
                    UnitPrice = unitPrice,
                    BasisCode = data.BasisCodes.GetValueOrDefault((key.BuyerCode, key.Order)),
                    Value = unitPrice * finalSectionQtyTotal,
                    Lines = lines
                });
            }

            return new ProductionSummaryStyleWiseDetailedReportServiceModel
            {
                StartDate = startDate,
                EndDate = endDate,
                FinalSectionCode = data.FinalSection.Code,
                FinalSectionDescription = data.FinalSection.Description,
                SectionCodes = data.Sections.Select(s => s.Code).ToList(),
                SectionDescriptions = data.Sections.Select(s => s.Description).ToList(),
                Rows = rows
            };
        }

        private async Task<decimal> ConvertQuantityAsync(string? fromUnit, string toUnit, decimal quantity)
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
