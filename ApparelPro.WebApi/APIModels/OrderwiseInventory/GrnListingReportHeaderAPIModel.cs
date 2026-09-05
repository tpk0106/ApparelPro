namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnListingReportHeaderAPIModel
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int? BuyerCode { get; set; }
        public string? BuyerName { get; set; }
        public string? Order { get; set; }
        public string? StoreCode { get; set; }
        public string? SupplierName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
        public string? TotalValueCurrency { get; set; }
    }
}
