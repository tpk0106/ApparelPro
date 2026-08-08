namespace ApparelPro.WebApi.Reports.Models
{
    public class OutstandingPurchaseOrderListReportAPIModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? BasisCode { get; set; }
        public List<OutstandingPurchaseOrderBasisGroupReportAPIModel> BasisGroups { get; set; } = new();
    }

    public class OutstandingPurchaseOrderBasisGroupReportAPIModel
    {
        public string BasisCode { get; set; } = "";
        public string BasisName { get; set; } = "";
        public List<OutstandingPurchaseOrderReportAPIModel> PurchaseOrders { get; set; } = new();
    }

    public class OutstandingPurchaseOrderReportAPIModel
    {
        public string PurchaseOrderNumber { get; set; } = "";
        public DateOnly? CreatedDate { get; set; }
        public TimeOnly? CreatedTime { get; set; }
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string? ProformaInvoiceNo { get; set; }
        public string CurrencyCode { get; set; } = "";
        public List<OutstandingPurchaseOrderGroupReportAPIModel> OutstandingGroups { get; set; } = new();
    }

    public class OutstandingPurchaseOrderGroupReportAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string TypeName { get; set; } = "";
        public string StyleCode { get; set; } = "";
    }
}
