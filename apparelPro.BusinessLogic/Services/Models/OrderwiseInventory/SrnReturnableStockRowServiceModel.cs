namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Supplier Return Note — one row per (Store, Item)
    // combination on the given Buyer/Order that currently has stock on hand. Unlike GTN's
    // picker (which also cross-checks a destination order), the ceiling here is simply the
    // current physical QtyInHand — legacy IN_SRN1.PRG's own limit ("m_qty >
    // qty_in_hd", "Attempt to Exceed Balance Quantity").
    public class SrnReturnableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // OrderwiseStock.QtyInHand at the time of lookup — the hard ceiling the commit
        // endpoint re-validates against. Kept as its own field (rather than reusing
        // QtyInHand) for the same clarity reason GtnTransferableStockRowServiceModel keeps
        // MaxTransferableQuantity separate from QtyInHand.
        public decimal MaxReturnableQuantity { get; set; }

        // Advisory only — never enforced server-side. QtyInHand minus ShadowBalance minus
        // StrnBalance (the same formula StoresRequisitionService.VerifyStockItemAvailabilityAsync
        // uses to compute its own "Avail"), i.e. what's left after everything already
        // reserved by open, not-yet-issued Stores Requisition Notes. Can be negative if
        // outstanding requisitions already exceed physical stock — that's a legitimate
        // state, not an error. Surfaced so the person posting a Supplier Return can see
        // they're about to draw down material another department is waiting on, without
        // being blocked from doing it (legacy IN_SRN1.PRG never checked this, and we're
        // deliberately not adding a new hard block it never had).
        public decimal NetAvailableAfterOutstandingRequisitions { get; set; }
    }
}
