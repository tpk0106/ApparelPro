namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnLineItemServiceModel
    {
        public int Buyer { get; set; }
        public string Order { get; set; } = null!;
        public int Type { get; set; }
        public string Style { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // 22-char composite key (matches PODetails.ItemCode)
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
