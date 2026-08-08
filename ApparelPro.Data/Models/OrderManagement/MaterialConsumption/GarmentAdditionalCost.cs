namespace ApparelPro.Data.Models.OrderManagement.MaterialConsumption
{
    // Replicates od_aitm.dbf (Additional Costs per Garment)
    public class GarmentAdditionalCost
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string AdditionalCostCode { get; set; } = null!; // add_cost - FK to AdditionalCost.Code

        // 22-char composite: StockCode(2) + ItemCode(4) + Feature1-4(4 each) - same convention
        // as StyleMaterialCostProfile.ItemCode / OrderwiseStockMaster / PODetails, not the split
        // columns StyleMaterialConsumptionLedger uses (od_aitm's own legacy index treats item_cd
        // as one opaque segment, matching the majority convention).
        public string ItemCode { get; set; } = null!;

        public string Color { get; set; } = string.Empty;   // blank = applies to whole style
        public string Size { get; set; } = string.Empty;    // blank = applies to whole style/colour
        public string StoreCode { get; set; } = null!;      // Basis code
        public string Currency { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }                // qty per garment
        public decimal Cost { get; set; }
        public bool IsCostPerGarment { get; set; }            // cost_gar: true = Cost is per-garment; false = Cost is the order's total, per-unit price is derived
        public bool IsSemiFinishedGarment { get; set; }       // semi_fin
    }
}
