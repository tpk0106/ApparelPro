namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockValuationMonthlyReportHeaderServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalLineItems { get; set; }
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalIssuedValue { get; set; }
    }
}
