using apparelPro.BusinessLogic.Services.interfaces.Reports.Production;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;
using ApparelPro.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Reports.Production
{
    // Replicates PR_MPRO1.PRG's "PRODUCTION SUMMARY - STYLE WISE" report -
    // see ProductionSummaryStyleWiseReportServiceModel for column semantics.
    // Order Qty and section quantities are totals across every production
    // line for the style; ProductionSummaryStyleWiseDetailedReportService is
    // the per-line breakdown variant, sharing data loading via
    // ProductionSummaryStyleWiseReportDataLoader.
    public class ProductionSummaryStyleWiseReportService : IProductionSummaryStyleWiseReportService
    {
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IUnitConversionService _unitConversionService;

        public ProductionSummaryStyleWiseReportService(
            ApparelProDbContext apparelProDbContext, IUnitConversionService unitConversionService)
        {
            _apparelProDbContext = apparelProDbContext;
            _unitConversionService = unitConversionService;
        }

        public async Task<ProductionSummaryStyleWiseReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate)
        {
            var data = await ProductionSummaryStyleWiseReportDataLoader.LoadAsync(_apparelProDbContext, startDate, endDate);

            var rows = new List<ProductionSummaryStyleWiseRowServiceModel>();
            foreach (var key in data.StyleKeys)
            {
                var style = data.Styles.GetValueOrDefault(key);
                var styleUnit = style?.Unit ?? "";

                var lineAllocations = data.Allocations.Where(a =>
                    a.BuyerCode == key.BuyerCode && a.Order == key.Order &&
                    a.TypeCode == key.TypeCode && a.StyleCode == key.StyleCode);

                decimal orderQty = 0;
                foreach (var allocation in lineAllocations)
                {
                    orderQty += await ConvertQuantityAsync(allocation.Unit, styleUnit, allocation.TotalQuantity);
                }

                var sectionQuantities = new List<ProductionSummaryStyleWiseSectionQtyServiceModel>();
                decimal finalSectionQty = 0;
                foreach (var section in data.Sections)
                {
                    var sectionEntries = data.Entries.Where(e =>
                        e.BuyerCode == key.BuyerCode && e.Order == key.Order && e.TypeCode == key.TypeCode &&
                        e.StyleCode == key.StyleCode && e.SectionCode == section.Code);

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
                        finalSectionQty = qty;
                }

                var unitPrice = style?.UnitPrice ?? 0;
                rows.Add(new ProductionSummaryStyleWiseRowServiceModel
                {
                    BuyerCode = key.BuyerCode,
                    BuyerName = data.BuyerNames.GetValueOrDefault(key.BuyerCode, key.BuyerCode.ToString()),
                    Order = key.Order,
                    StyleCode = key.StyleCode,
                    Description = data.Descriptions.GetValueOrDefault((key.BuyerCode, key.Order)),
                    OrderQty = orderQty,
                    Unit = styleUnit,
                    SectionQuantities = sectionQuantities,
                    UnitPrice = unitPrice,
                    BasisCode = data.BasisCodes.GetValueOrDefault((key.BuyerCode, key.Order)),
                    Value = unitPrice * finalSectionQty
                });
            }

            return new ProductionSummaryStyleWiseReportServiceModel
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
