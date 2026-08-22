namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryDailyLineAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public string LineCode { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public List<ProductionSummaryDailySectionTotalAPIModel> Sections { get; set; } = new();
    }
}
