namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnListingReportLineAPIModel
    {
        public DateOnly TransactionDate { get; set; }
        public string GrnNumber { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
        public string? PoNumber { get; set; }
        public string? LcNumber { get; set; }
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
    }
}
