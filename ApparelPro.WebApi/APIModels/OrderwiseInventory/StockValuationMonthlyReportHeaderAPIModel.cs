namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockValuationMonthlyReportHeaderAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalLineItems { get; set; }
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalIssuedValue { get; set; }
    }
}
