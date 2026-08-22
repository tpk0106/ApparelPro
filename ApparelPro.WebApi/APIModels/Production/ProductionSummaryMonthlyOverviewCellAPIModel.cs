namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlyOverviewCellAPIModel
    {
        public decimal EstQuantity { get; set; }
        public decimal ActQuantity { get; set; }
        public decimal CumEstQuantity { get; set; }
        public decimal CumActQuantity { get; set; }
    }
}
