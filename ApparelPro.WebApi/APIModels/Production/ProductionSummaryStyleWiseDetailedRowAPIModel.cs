namespace ApparelPro.WebApi.APIModels.Production
{
    public class ProductionSummaryStyleWiseDetailedRowAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string StyleCode { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public string? BasisCode { get; set; }
        public decimal Value { get; set; }
        public List<ProductionSummaryStyleWiseDetailedLineAPIModel> Lines { get; set; } = new();
    }
}
