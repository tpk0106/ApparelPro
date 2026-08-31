namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockValuationMonthlyReportLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal IssuedQuantity { get; set; }
        public decimal IssuedValue { get; set; }
    }
}
