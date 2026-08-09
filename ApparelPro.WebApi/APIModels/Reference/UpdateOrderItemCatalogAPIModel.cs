namespace ApparelPro.WebApi.APIModels.Reference
{
    public class UpdateOrderItemCatalogAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
