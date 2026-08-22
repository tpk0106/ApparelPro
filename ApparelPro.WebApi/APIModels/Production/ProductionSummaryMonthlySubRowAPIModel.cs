namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryMonthlySubRowAPIModel
    {
        public List<ProductionSummaryMonthlyCellAPIModel> LineCells { get; set; } = new();
        public decimal TotalEstQuantity { get; set; }
        public decimal TotalActQuantity { get; set; }
        public decimal TotalCumEstQuantity { get; set; }
        public decimal TotalCumActQuantity { get; set; }
    }
}
