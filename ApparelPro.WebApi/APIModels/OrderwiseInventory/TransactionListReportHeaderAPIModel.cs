namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class TransactionListReportHeaderAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionTypeName { get; set; }
        public string? ItemCodePrefix { get; set; }
        public int TotalLineItems { get; set; }
        public decimal TotalValue { get; set; }
        public string? TotalValueCurrency { get; set; }
    }
}
