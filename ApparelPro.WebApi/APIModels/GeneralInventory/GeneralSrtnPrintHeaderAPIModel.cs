namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSrtnPrintHeaderAPIModel
    {
        public string SrtnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; } = "";
        public string StockType { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
