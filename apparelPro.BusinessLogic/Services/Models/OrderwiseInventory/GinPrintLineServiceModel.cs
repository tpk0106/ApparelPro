namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        // Legacy column label is "Basis" (store_cd) - same naming convention as
        // StrnPrintLineServiceModel, kept as StoreCode to avoid colliding with the
        // unrelated Basis/GarmentType costing concept elsewhere in the system.
        public string StoreCode { get; set; } = null!;
    }
}
