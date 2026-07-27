namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Goods Transfer Note — one row per (Store, Item)
    // combination on the From Buyer/Order that (a) still has stock on hand to transfer and
    // (b) already exists in the To Buyer/Order's stock too. Legacy IN_GTN1.PRG only checks
    // (b) at commit time and rejects the whole line with an error dialog after the operator
    // has already typed it in ("Item Code not found in [To Buyer/Order]") — pre-filtering the
    // picker to only transferable items is a UX improvement over that late rejection, while
    // the underlying business rule (destination stock row must already exist) is unchanged.
    public class GtnTransferableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }

        // OrderwiseStock.QtyInHand on the From side at the time of lookup — the hard ceiling
        // the commit endpoint re-validates against (legacy: "Attempt to Exceed Balance
        // Quantity"). Kept as its own field (rather than reusing QtyInHand) for the same
        // clarity reason RtnReturnableStockRowServiceModel.MaxReturnableQuantity is separate
        // from QtyInHand.
        public decimal MaxTransferableQuantity { get; set; }
    }
}
