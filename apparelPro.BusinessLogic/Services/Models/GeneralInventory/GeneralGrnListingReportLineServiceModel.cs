namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGrnListingReportLineServiceModel
    {
        public DateOnly TransactionDate { get; set; }
        public string GrnNumber { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
        public string? PoNumber { get; set; }
        public string? SupplierCode { get; set; }
        public string SupplierName { get; set; } = "";
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
