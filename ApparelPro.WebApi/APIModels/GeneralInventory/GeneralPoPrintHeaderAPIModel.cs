namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPoPrintHeaderAPIModel
    {
        public string PoNumber { get; set; } = null!;
        public DateTime? OrderDate { get; set; }
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; } = "";
        public string SupplierAddress { get; set; } = "";
        public string CurrencyCode { get; set; } = "";
        public string? ProformaInvoiceNo { get; set; }
        public DateTime? ProformaInvoiceDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
