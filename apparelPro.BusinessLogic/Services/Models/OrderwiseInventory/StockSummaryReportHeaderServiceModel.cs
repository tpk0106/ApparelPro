namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockSummaryReportHeaderServiceModel
    {
        public string Currency1 { get; set; } = null!;
        public string Currency2 { get; set; } = null!;
        public decimal GrandTotalCurrency1 { get; set; }
        public decimal GrandTotalCurrency2 { get; set; }
        public int TotalStockTypes { get; set; }
    }
}
