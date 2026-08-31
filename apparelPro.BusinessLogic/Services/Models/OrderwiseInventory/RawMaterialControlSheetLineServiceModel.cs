namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RawMaterialControlSheetLineServiceModel
    {
        public string StockCode { get; set; } = null!;
        public string StockDescription { get; set; } = "";
        public string ItemCode { get; set; } = null!; // 22-char composite
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal TotalConsumption { get; set; }
        public decimal ExactConsumption { get; set; }
        public decimal TotalOrderQuantity { get; set; }
        public decimal TotalReceivedQuantity { get; set; }
        public decimal TotalIssuedQuantity { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "";
    }
}
