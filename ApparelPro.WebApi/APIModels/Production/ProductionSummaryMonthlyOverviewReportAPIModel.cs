namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlyOverviewReportAPIModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = "";
        public List<string> LineCodes { get; set; } = new();
        public List<string> LineDescriptions { get; set; } = new();
        public List<ProductionSummaryMonthlyOverviewDayRowAPIModel> Days { get; set; } = new();
    }
}
