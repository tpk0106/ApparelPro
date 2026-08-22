namespace apparelPro.BusinessLogic.Services.Models.Production.ILineProductionSummaryReportService
{
    // Replicates PR_LPROD.PRG's "LINE PRODUCTION SUMMARY" report (Reports ->
    // Line Production Summary). Scoped to the Final section only - one row
    // per Production Line, with two quantity columns:
    //  - PeriodQty: Final-section quantity produced within [StartDate, EndDate].
    //  - CumulativeQty: Final-section quantity produced from all recorded
    //    history through EndDate - deliberately NOT bounded by StartDate.
    //    Traced directly from the legacy loop, which only checks the upper
    //    bound (date <= endDate) while accumulating array[1]; this is a
    //    genuine "all-time to date" total, preserved as-is per user
    //    confirmation rather than "corrected" to a period-only figure.
    public class LineProductionSummaryRowServiceModel
    {
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public decimal PeriodQty { get; set; }
        public decimal CumulativeQty { get; set; }
    }

    public class LineProductionSummaryReportServiceModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<LineProductionSummaryRowServiceModel> Rows { get; set; } = new();
        public decimal TotalPeriodQty { get; set; }
        public decimal TotalCumulativeQty { get; set; }
    }
}
