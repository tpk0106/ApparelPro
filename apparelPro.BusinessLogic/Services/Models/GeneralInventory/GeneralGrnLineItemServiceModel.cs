namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnLineItemServiceModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; } // entered in header CurrencyCode
    }
}
