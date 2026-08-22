namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryStyleWiseReportAPIModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<string> SectionCodes { get; set; } = new();
        public List<string> SectionDescriptions { get; set; } = new();
        public List<ProductionSummaryStyleWiseRowAPIModel> Rows { get; set; } = new();
    }
}
