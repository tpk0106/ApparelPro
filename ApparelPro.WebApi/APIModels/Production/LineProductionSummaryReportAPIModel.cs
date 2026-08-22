namespace ApparelPro.WebApi.APIModels.Production
{
    public class LineProductionSummaryReportAPIModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string FinalSectionCode { get; set; } = null!;
        public string FinalSectionDescription { get; set; } = null!;
        public List<LineProductionSummaryRowAPIModel> Rows { get; set; } = new();
        public decimal TotalPeriodQty { get; set; }
        public decimal TotalCumulativeQty { get; set; }
    }
}
