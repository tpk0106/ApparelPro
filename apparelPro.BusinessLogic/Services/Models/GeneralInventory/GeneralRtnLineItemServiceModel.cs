namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRtnLineItemServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
