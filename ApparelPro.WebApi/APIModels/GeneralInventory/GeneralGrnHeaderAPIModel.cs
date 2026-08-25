namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnHeaderAPIModel
    {
        public string GrnNumber { get; set; } = null!;
        public string PoNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string SupplierCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
    }
}
