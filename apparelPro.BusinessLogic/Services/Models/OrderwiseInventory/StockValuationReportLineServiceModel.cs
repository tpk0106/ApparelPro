namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockValuationReportLineServiceModel
    {
        public string StockTypeCode { get; set; } = "";
        public string StockTypeDescription { get; set; } = "";

        // Blank on a subtotal/grand-total row.
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal IssuedValue { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal BalanceValue { get; set; }

        // "Item" | "StockTypeSubtotal" | "GrandTotal" - same convention as
        // GeneralStockSummaryReportLineServiceModel.
        public string RowType { get; set; } = "Item";
    }
}
