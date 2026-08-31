namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGrnListingReportHeaderAPIModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string? StoreCode { get; set; }
        public string? StoreDescription { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
    }
}
