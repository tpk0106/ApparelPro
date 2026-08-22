namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryDailyReportAPIModel
    {
        public DateOnly Date { get; set; }
        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public List<ProductionSummaryDailyLineAPIModel> Lines { get; set; } = new();
        public decimal TotalOrderQuantity { get; set; }
        public List<ProductionSummaryDailySectionTotalAPIModel> Totals { get; set; } = new();
    }
}
