namespace ApparelPro.Data.Models.OrderwiseInventory
{
    // Replicates gi_stref.dbf (General Inventory Stock Reference) - a general item
    // catalog (604 legacy records) used as a fallback description lookup when a style's
    // own material budget (StyleMaterialCostProfile / od_sacc2) has no matching row for
    // an item. Uses the single 22-char composite ItemCode convention already
    // established for StyleMaterialCostProfile/GarmentAdditionalCost (StockCode(2) +
    // ItemCode(4) + Feature1-4(4 each) concatenated), not 6 separate columns.
    public class GeneralStockReference
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
    }
}
