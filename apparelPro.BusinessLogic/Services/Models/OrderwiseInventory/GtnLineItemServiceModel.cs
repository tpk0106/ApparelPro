namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GtnLineItemServiceModel
    {
        // "Basis" in the legacy screen labels — matches OrderwiseStock.StoreCode. Must be the
        // same Basis on both the From and To side (legacy IN_GTN1.PRG never asks for two).
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // 22-char composite key
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
