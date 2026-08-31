namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockSummaryReportHeaderAPIModel
    {
        public string Currency1 { get; set; } = null!;
        public string Currency2 { get; set; } = null!;
        public decimal GrandTotalCurrency1 { get; set; }
        public decimal GrandTotalCurrency2 { get; set; }
        public int TotalStockTypes { get; set; }
    }
}
