namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Stock Adjustment Note — one row per (Store, Item)
    // combination on the given Buyer/Order, REGARDLESS of current QtyInHand (unlike SRN/
    // DGN's "qty_in_hd > 0" filter). A stock take needs to be able to correct an item's
    // count up from 0 just as much as down from some positive value, so every row is
    // shown — matching legacy IN_SAN1.PRG's unconditional "copy whil buyer+order = ...".
    public class SanAdjustableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Current physical count — informational context only. There is deliberately no
        // "MaxAdjustableQuantity" ceiling field here (unlike every other note type): SAN
        // has no ceiling, legacy never validates the new count against the old one.
        public decimal QtyInHand { get; set; }
    }
}
