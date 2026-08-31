namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockSummaryReportLineServiceModel
    {
        public string StockTypeCode { get; set; } = null!;
        public string StockTypeDescription { get; set; } = "";

        // Blank on a subtotal/grand-total row.
        public string StoreCode { get; set; } = "";

        public decimal ValueInCurrency1 { get; set; }
        public decimal ValueInCurrency2 { get; set; }

        // "Store" | "StockTypeSubtotal" | "GrandTotal"
        public string RowType { get; set; } = "Store";
    }
}
