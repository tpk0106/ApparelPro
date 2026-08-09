namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // Powers the item picker for a new Additional Issue Note - one row per (Store, Item)
    // on the given Buyer/Order that also has a matching StyleMaterialCostProfile row
    // (legacy IN_AIN1.PRG hard-blocks entry of any item with "No Material Consumptions
    // made for Item", so items without a profile are left out of the picker entirely
    // rather than being offered and then rejected).
    public class AinIssuableStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // "Basis"
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal OrderedQuantity { get; set; }
        public decimal ShadowBalance { get; set; }
        public decimal ToDateIssued { get; set; }
        public decimal QtyInHand { get; set; }

        // OrderedQuantity - ShadowBalance - ToDateIssued. Mirrors legacy's
        // "Balance Qty : ord_qty-shdw_bal-to_dt_iss" prompt shown per line during entry -
        // informational context for the frontend, the real enforcement happens server-side
        // on commit.
        public decimal AvailableForIssue { get; set; }
    }
}
