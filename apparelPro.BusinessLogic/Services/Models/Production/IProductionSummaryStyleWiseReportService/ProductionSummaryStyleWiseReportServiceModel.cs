namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService
{
    // Replicates PR_MPRO1.PRG's "PRODUCTION SUMMARY - STYLE WISE" report
    // (Reports -> Production Summary (Style Wise)). One row per
    // Buyer/Order/Type/Style active within the given date range, with one
    // Prod. Total Qty. column per Section (dynamic, from the Sections
    // master). Order Qty is summed across every ProductionLineAllocation
    // for the style; the per-line breakdown is what
    // ProductionSummaryStyleWiseDetailedReportServiceModel is for.
    public class ProductionSummaryStyleWiseSectionQtyServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    public class ProductionSummaryStyleWiseRowServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string StyleCode { get; set; } = null!;

        // From PurchaseOrders.Description - the legacy od_po.desc free-text
        // order description (confirmed by tracing the Clipper work-area
        // context in PR_MPRO1.PRG, which reads it right after a SEEK on
        // od_po - same field already migrated for the Daily Summary report).
        public string? Description { get; set; }

        public decimal OrderQty { get; set; }
        public string Unit { get; set; } = null!;

        public List<ProductionSummaryStyleWiseSectionQtyServiceModel> SectionQuantities { get; set; } = new();

        public decimal UnitPrice { get; set; }
        public string? BasisCode { get; set; }

        // UnitPrice * the quantity produced in the Final section (per the
        // user's chosen semantics: value is realized against completed
        // production, not intermediate section throughput).
        public decimal Value { get; set; }
    }

    public class ProductionSummaryStyleWiseReportServiceModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public List<ProductionSummaryStyleWiseRowServiceModel> Rows { get; set; } = new();
    }
}
