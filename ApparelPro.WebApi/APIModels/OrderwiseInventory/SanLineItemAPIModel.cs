namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanLineItemAPIModel
    {
        // "Basis" in the legacy screen labels — matches OrderwiseStock.StoreCode.
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!; // 22-char composite key
        public string Unit { get; set; } = null!;

        // See SanLineItemServiceModel's comment — a SET, not a movement delta.
        public decimal AdjustedQuantity { get; set; }
    }
}
