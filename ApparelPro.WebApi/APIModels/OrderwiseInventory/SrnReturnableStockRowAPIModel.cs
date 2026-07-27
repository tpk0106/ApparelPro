namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SrnReturnableStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // Hard ceiling for the Quantity field on the entry grid — legacy: "Attempt to
        // Exceed Balance Quantity."
        public decimal MaxReturnableQuantity { get; set; }

        // Advisory only, never enforced. See SrnReturnableStockRowServiceModel's comment
        // for the exact formula — used purely to render a non-blocking warning.
        public decimal NetAvailableAfterOutstandingRequisitions { get; set; }
    }
}
