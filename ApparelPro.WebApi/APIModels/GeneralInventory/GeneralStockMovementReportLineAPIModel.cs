namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockMovementReportLineAPIModel
    {
        public DateOnly TransactionDate { get; set; }
        public TimeOnly? TransactionTime { get; set; }
        public string TransactionTypeCode { get; set; } = null!;
        public string DocumentTypeDescription { get; set; } = "";
        public string DocumentNumber { get; set; } = null!;
        public string Status { get; set; } = "";
        public string SourceTarget { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
