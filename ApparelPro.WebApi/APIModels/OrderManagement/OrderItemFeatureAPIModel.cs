namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class OrderItemFeatureAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string? Feature1 { get; set; } // Points to FeatureCode (e.g. 'TY', 'CL')
        public string? Feature2 { get; set; }
        public string? Feature3 { get; set; }
        public string? Feature4 { get; set; }
        public decimal? CostPerUnit { get; set; }
    }
}
