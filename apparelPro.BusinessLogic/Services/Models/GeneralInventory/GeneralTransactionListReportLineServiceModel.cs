namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralTransactionListReportLineServiceModel
    {
        public DateOnly TransactionDate { get; set; }
        public TimeOnly? TransactionTime { get; set; }
        public string TransactionTypeCode { get; set; } = null!;
        public string DocumentTypeDescription { get; set; } = "";
        public string DocumentNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
    }
}
