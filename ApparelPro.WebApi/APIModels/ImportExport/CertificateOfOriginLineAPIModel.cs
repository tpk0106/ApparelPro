namespace ApparelPro.WebApi.APIModels.ImportExport
{
    public class CertificateOfOriginLineAPIModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int ItemNo { get; set; }
        public string ShippingMarks { get; set; } = "";
        public string PackageTypeQuantity { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string HsCode { get; set; } = "";
        public decimal NettWeight { get; set; }
        public decimal GrossWeight { get; set; }
    }
}
