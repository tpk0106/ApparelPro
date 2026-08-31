namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnPrintHeaderAPIModel
    {
        public string ArnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
        public string SubContractorCode { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
