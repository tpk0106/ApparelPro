namespace ApparelPro.WebApi.APIModels.Production
{
    public class LineProductionSummaryRowAPIModel
    {
        public string LineCode { get; set; } = null!;
        public string LineDescription { get; set; } = "";
        public decimal PeriodQty { get; set; }
        public decimal CumulativeQty { get; set; }
    }
}
