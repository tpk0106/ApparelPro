using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;

namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseDetailedReportService
{
    // Per-line companion to ProductionSummaryStyleWiseReportServiceModel -
    // same report, but Order Qty and section quantities are broken down per
    // production Line instead of summed to one total per style. Reuses the
    // shared ProductionSummaryStyleWiseSectionQtyServiceModel cell shape.
    public class ProductionSummaryStyleWiseDetailedLineServiceModel
    {
        public string LineCode { get; set; } = null!;
        public decimal OrderQty { get; set; }
        public List<ProductionSummaryStyleWiseSectionQtyServiceModel> SectionQuantities { get; set; } = new();
    }

    public class ProductionSummaryStyleWiseDetailedRowServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string StyleCode { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public string? BasisCode { get; set; }

        // UnitPrice * total Final-section quantity across every line for
        // this style (same semantics as the non-detailed report's Value).
        public decimal Value { get; set; }

        public List<ProductionSummaryStyleWiseDetailedLineServiceModel> Lines { get; set; } = new();
    }

    public class ProductionSummaryStyleWiseDetailedReportServiceModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public List<ProductionSummaryStyleWiseDetailedRowServiceModel> Rows { get; set; } = new();
    }
}
