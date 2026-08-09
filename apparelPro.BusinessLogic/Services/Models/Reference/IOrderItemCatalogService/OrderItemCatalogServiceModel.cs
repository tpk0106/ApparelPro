namespace apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService
{
    public class OrderItemCatalogServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string StockDescription { get; set; } = string.Empty;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
