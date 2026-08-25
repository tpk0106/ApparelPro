namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSrtnPrintHeaderServiceModel
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
