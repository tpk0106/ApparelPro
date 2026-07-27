namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Goods Return Note — one row per (Store, Item)
    // combination that still has something returnable for this Buyer/Order.
    public class RtnReturnableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // OrderwiseStock.ToDateIssued at the time of lookup — the hard ceiling the
        // commit endpoint re-validates against (legacy: "Return Quantity cannot be
        // greater than Total issues").
        public decimal MaxReturnableQuantity { get; set; }
    }
}
