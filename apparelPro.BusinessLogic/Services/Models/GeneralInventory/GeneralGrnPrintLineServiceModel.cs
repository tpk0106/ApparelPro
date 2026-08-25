namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnPrintLineServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
