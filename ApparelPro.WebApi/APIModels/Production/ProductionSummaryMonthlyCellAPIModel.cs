namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlyCellAPIModel
    {
        public string? StyleCode { get; set; }
        public decimal? EstQuantity { get; set; }
        public decimal? ActQuantity { get; set; }
        public decimal CumEstQuantity { get; set; }
        public decimal CumActQuantity { get; set; }
    }
}
