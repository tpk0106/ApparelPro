namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralStockValuationReportLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal QtyInHand { get; set; }
        public decimal Value { get; set; }
        public decimal DamagedQuantity { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }

        // zdiv(value, qty_in_hd) in the legacy source - 0 rather than a divide-by-zero
        // when QtyInHand is 0.
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = null!;
    }
}
