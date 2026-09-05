namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class TransactionListReportLineAPIModel
    {
        public string TransactionType { get; set; } = null!;
        public string TransactionTypeName { get; set; } = "";
        public DateOnly TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
    }
}
