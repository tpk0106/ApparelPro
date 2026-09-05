namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GrnPrintHeaderServiceModel
    {
        public string GrnNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public int? SupplierCode { get; set; }
        public string SupplierName { get; set; } = null!;
        public string PoNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
