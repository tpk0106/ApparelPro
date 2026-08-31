namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockReorderReportLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;

        // zdiv(value, qty_in_hd) in the legacy source - 0 rather than a divide-by-zero
        // when QtyInHand is 0.
        public decimal AveragePrice { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
    }
}
