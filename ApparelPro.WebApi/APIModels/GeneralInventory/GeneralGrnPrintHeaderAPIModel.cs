namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnPrintHeaderAPIModel
    {
        public string GrnNumber { get; set; } = null!;
        public string PoNumber { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public string CurrencyCode { get; set; } = "";
        public string? InvoiceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
