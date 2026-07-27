namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Damaged Goods Note — one row per (Store, Item)
    // combination on the given Buyer/Order that currently has stock on hand. The ceiling
    // here is simply the current physical QtyInHand — legacy IN_DGN1.PRG's own limit
    // ("m_qty > qty_in_hd", "Attempt to Exceed Balance Quantity"), same as SRN's picker.
    public class DgnDamageableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // OrderwiseStock.QtyInHand at the time of lookup — the hard ceiling the commit
        // endpoint re-validates against.
        public decimal MaxDamageableQuantity { get; set; }
    }
}
