namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class CreateOrderItemFeatureMappingAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;

        public string? Feature1Type { get; set; }
        public string? Feature2Type { get; set; }
        public string? Feature3Type { get; set; }
        public string? Feature4Type { get; set; }

        public decimal? CostPerUnit { get; set; }
    }
}
