namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralTransactionListReportHeaderServiceModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // Null/empty means "ALL" (legacy choice 10) - every transaction type listed.
        public string? TransactionTypeCode { get; set; }
        public string? ItemCodePrefix { get; set; }
        public int TotalLineItems { get; set; }
    }
}
