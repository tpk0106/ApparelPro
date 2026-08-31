namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ItemWiseStockBalanceLineServiceModel
    {
        public string StockTypeCode { get; set; } = "";
        public string StockTypeDescription { get; set; } = "";
        public string ItemGroupCode { get; set; } = ""; // 6-char Stock+Item code
        public string ItemGroupDescription { get; set; } = "";

        // Blank on a subtotal/grand-total row.
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal BalanceValue { get; set; }

        // "Item" | "ItemGroupSubtotal" | "StockTypeSubtotal" | "GrandTotal"
        public string RowType { get; set; } = "Item";
    }
}
