namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinLineItemServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
