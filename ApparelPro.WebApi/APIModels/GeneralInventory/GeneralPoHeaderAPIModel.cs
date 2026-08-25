namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPoHeaderAPIModel
    {
        public string PoNumber { get; set; } = "";
        public bool IsNewPurchaseOrder { get; set; }
        public string SupplierCode { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string BasisCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public string? ProformaInvoiceNo { get; set; }
        public DateTime? ProformaInvoiceDate { get; set; }
    }
}
