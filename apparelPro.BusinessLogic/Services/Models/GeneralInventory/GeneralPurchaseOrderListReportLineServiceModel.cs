namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralPurchaseOrderListReportLineServiceModel
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
