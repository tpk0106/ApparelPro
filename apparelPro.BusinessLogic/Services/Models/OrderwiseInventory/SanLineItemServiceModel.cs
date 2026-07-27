namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SanLineItemServiceModel
    {
        // "Basis" in the legacy screen labels — matches OrderwiseStock.StoreCode.
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // 22-char composite key

        public string Unit { get; set; } = null!;

        // Deliberately NOT named "Quantity" like every other note type's line item.
        // Every other note's Quantity is a MOVEMENT — added to or subtracted from
        // QtyInHand. SAN is fundamentally different: legacy IN_SAN1.PRG directly
        // REPLACES OrderwiseStock.QtyInHand with this value (a stock-take correction),
        // it never adds/subtracts. Naming it AdjustedQuantity makes that distinction
        // explicit rather than reusing a name that implies a delta.
        public decimal AdjustedQuantity { get; set; }
    }
}
