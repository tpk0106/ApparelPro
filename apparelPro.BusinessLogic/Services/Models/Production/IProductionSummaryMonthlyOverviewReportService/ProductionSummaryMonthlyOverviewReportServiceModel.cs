namespace apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyOverviewReportService
{
    // A simplified companion to ProductionSummaryMonthlyReportServiceModel
    // (Reports -> B. Production Summary (Monthly)) - not a legacy screen.
    // Same data (Estimate vs Actual by Line, for the Final section, for a
    // month), but a line's multiple same-day style entries are summed into
    // one cell instead of stacked into extra rows, so this is always a
    // fixed one-row-per-day grid.
    public class ProductionSummaryMonthlyOverviewCellServiceModel
    {
        public decimal EstQuantity { get; set; }
        public decimal ActQuantity { get; set; }
        public decimal CumEstQuantity { get; set; }
        public decimal CumActQuantity { get; set; }
    }

    public class ProductionSummaryMonthlyOverviewDayRowServiceModel
    {
        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayDescription { get; set; }

        // Aligned to ProductionSummaryMonthlyOverviewReportServiceModel.LineCodes order.
        public List<ProductionSummaryMonthlyOverviewCellServiceModel> LineCells { get; set; } = new();

        public decimal TotalEstQuantity { get; set; }
        public decimal TotalActQuantity { get; set; }
        public decimal TotalCumEstQuantity { get; set; }
        public decimal TotalCumActQuantity { get; set; }
    }

    public class ProductionSummaryMonthlyOverviewReportServiceModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = "";
        public List<string> LineCodes { get; set; } = new();
        public List<string> LineDescriptions { get; set; } = new();
        public List<ProductionSummaryMonthlyOverviewDayRowServiceModel> Days { get; set; } = new();
    }
}
