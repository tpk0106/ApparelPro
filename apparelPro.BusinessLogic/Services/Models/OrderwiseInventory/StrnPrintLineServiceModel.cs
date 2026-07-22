namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StrnPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }

        // Legacy column label was "Basis" (store_cd) — kept as StoreCode here to avoid
        // colliding with the unrelated Basis/GarmentType costing concept elsewhere in
        // the system; the frontend can decide how to label the column.
        public string StoreCode { get; set; } = null!;
    }
}
