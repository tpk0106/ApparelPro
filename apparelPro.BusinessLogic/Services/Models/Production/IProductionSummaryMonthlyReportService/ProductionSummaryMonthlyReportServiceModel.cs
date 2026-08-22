namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyReportService
{
    // Replicates PR_MPROD.PRG's "MONTHLY PRODUCTION SUMMARY" report (Reports
    // -> B. Production Summary (Monthly)) faithfully, including its dynamic
    // sub-row stacking when a line has more than one style's Estimate/Actual
    // entry on the same day. Cumulative totals are per-Line for the whole
    // month, not per-style - style is a display label only (legacy's own
    // per-style cumulative reset is commented out in the source).
    public class ProductionSummaryMonthlyCellServiceModel
    {
        public string? StyleCode { get; set; }
        public decimal? EstQuantity { get; set; }
        public decimal? ActQuantity { get; set; }

        // Always populated (this Line's running total through this day),
        // even on a pass where this specific Line had no new entry.
        public decimal CumEstQuantity { get; set; }
        public decimal CumActQuantity { get; set; }
    }

    public class ProductionSummaryMonthlySubRowServiceModel
    {
        // Aligned to ProductionSummaryMonthlyReportServiceModel.LineCodes order.
        public List<ProductionSummaryMonthlyCellServiceModel> LineCells { get; set; } = new();

        public decimal TotalEstQuantity { get; set; }
        public decimal TotalActQuantity { get; set; }
        public decimal TotalCumEstQuantity { get; set; }
        public decimal TotalCumActQuantity { get; set; }
    }

    public class ProductionSummaryMonthlyDayRowServiceModel
    {
        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }

        // Normally 1 row; 2+ only on a day where some line had more than
        // one style's Estimate/Actual entry (a real style changeover).
        public List<ProductionSummaryMonthlySubRowServiceModel> SubRows { get; set; } = new();
    }

    public class ProductionSummaryMonthlyReportServiceModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = "";
        public List<string> LineCodes { get; set; } = new();
        public List<string> LineDescriptions { get; set; } = new();
        public List<ProductionSummaryMonthlyDayRowServiceModel> Days { get; set; } = new();
    }
}
