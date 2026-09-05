namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SrnPrintHeaderAPIModel
    {
        public string SrnNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
