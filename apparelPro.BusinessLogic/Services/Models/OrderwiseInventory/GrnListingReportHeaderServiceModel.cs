namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnListingReportHeaderServiceModel
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int? BuyerCode { get; set; }
        public string? Order { get; set; }
        public string? StoreCode { get; set; }
        public string? SupplierName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
        public string? TotalValueCurrency { get; set; }
    }
}
