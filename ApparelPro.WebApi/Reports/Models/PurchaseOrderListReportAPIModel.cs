namespace ApparelPro.WebApi.Reports.Models
{
    public class PurchaseOrderListReportAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string? ProformaInvoiceNo { get; set; }
        public DateOnly? ProformaInvoiceDate { get; set; }
        public string CurrencyCode { get; set; } = "";

        public List<PurchaseOrderListLineReportAPIModel> Lines { get; set; } = new();
    }

    public class PurchaseOrderListLineReportAPIModel
    {
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal OrderQuantity { get; set; }
        public string OrderUnit { get; set; } = "";
        public decimal UnitPrice { get; set; }

        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";
    }
}
