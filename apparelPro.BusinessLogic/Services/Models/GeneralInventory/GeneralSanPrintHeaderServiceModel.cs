namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSanPrintHeaderServiceModel
    {
        public string SanNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
