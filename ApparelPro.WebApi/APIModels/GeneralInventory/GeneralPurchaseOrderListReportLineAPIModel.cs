namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralPurchaseOrderListReportLineAPIModel
    {
        public string PoNumber { get; set; } = null!;
        public DateOnly? OrderDate { get; set; }
        public TimeOnly? OrderTime { get; set; }
        public string SupplierName { get; set; } = "";
        public string? BasisCode { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? CurrencyCode { get; set; }
        public string PreparedBy { get; set; } = "";
    }
}
