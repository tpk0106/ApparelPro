namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSanPrintHeaderAPIModel
    {
        public string SanNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
