namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockMovementItemReportLineAPIModel
    {
        public DateTime TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string TransactionType { get; set; } = null!;
        public string TransactionTypeName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal BalanceAfter { get; set; }
    }
}
