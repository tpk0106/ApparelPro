namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSrtnHeaderAPIModel
    {
        public string SrtnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public int SupplierCode { get; set; }
        public string StockType { get; set; } = null!;
    }
}
