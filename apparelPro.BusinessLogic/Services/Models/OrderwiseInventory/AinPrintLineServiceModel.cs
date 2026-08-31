namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class AinPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
