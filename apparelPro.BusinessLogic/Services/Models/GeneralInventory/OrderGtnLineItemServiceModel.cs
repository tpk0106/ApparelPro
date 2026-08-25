namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class OrderGtnLineItemServiceModel
    {
        public string StoreCode { get; set; } = null!; // a General store (GeneralStores.Code)
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
