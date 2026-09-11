namespace ApparelPro.Data.Models.ImportExport
{
    // One row per item on the Certificate of Origin (boxes 7-12 of the real
    // EXP 5 form) - unlike Commercial Invoice's free-text Detail memo, this
    // form's item description is fully columnar, so no memo fallback here.
    public class CertificateOfOriginLine
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
