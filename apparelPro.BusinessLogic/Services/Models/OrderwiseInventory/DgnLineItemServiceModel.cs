namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DgnLineItemServiceModel
    {
        // "Basis" in the legacy screen labels — matches OrderwiseStock.StoreCode.
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // 22-char composite key
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
