namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionAnalysisSummaryReportService
{
    // Replicates PR_MPRO2.PRG's "PRODUCTION ANALYSIS SUMMARY - (For Style)"
    // report. One row per (Date, Line) combination for the style's entire
    // production history, with one quantity column per Section.
    //
    // Deliberate fix (per user confirmation): legacy prints each detail
    // record into the next column slot by a running counter rather than
    // matching it to its actual Section - correct only when every section
    // has an entry, in section-code order, for every date+line. Here every
    // cell is instead keyed to its real SectionCode, so a missing section
    // shows 0 in the right place instead of shifting later columns left.
    //
    // Two summary metrics preserved with their literal (non-obvious) legacy
    // definition: both count (Date, Line) combinations with nonzero
    // quantity, not calendar days - a date with two lines producing counts
    // as 2, matching PR_MPRO2.PRG's own f_qty_days/no_days accumulation.
    public class ProductionAnalysisSectionQtyServiceModel
    {
        public string SectionCode { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    public class ProductionAnalysisRowServiceModel
    {
        public DateOnly Date { get; set; }
        public string LineCode { get; set; } = null!;
        public List<ProductionAnalysisSectionQtyServiceModel> SectionQuantities { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class ProductionAnalysisSummaryReportServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = null!;

        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;

        public List<ProductionAnalysisRowServiceModel> Rows { get; set; } = new();
        public List<ProductionAnalysisSectionQtyServiceModel> SectionTotals { get; set; } = new();

        // TotalQty(FinalSection) / FinalOutputProductionDays.
        public decimal AverageProductionQuantityOnFinalOutput { get; set; }

        // Count of (Date, Line) combinations with nonzero Final-section qty.
        public int FinalOutputProductionDays { get; set; }

        // Count of (Date, Line) combinations with nonzero total qty across
        // every section.
        public int TotalDaysTakenForProduction { get; set; }
    }
}
