namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralTransactionListReportHeaderAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? TransactionTypeCode { get; set; }
        public string? ItemCodePrefix { get; set; }
        public int TotalLineItems { get; set; }
    }
}
