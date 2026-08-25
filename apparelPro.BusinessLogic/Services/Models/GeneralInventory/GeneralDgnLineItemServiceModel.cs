namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralDgnLineItemServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
