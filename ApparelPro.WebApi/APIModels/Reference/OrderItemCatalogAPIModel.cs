namespace ApparelPro.WebApi.APIModels.Reference
{
    public class OrderItemCatalogAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string StockDescription { get; set; } = string.Empty;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
