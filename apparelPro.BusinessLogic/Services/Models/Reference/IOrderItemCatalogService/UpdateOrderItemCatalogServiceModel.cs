namespace apparelPro.BusinessLogic.Services.Models.Reference.IOrderItemCatalogService
{
    public class UpdateOrderItemCatalogServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
