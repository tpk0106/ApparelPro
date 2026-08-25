namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGtnHeaderServiceModel
    {
        public string GtnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string FromStoreCode { get; set; } = null!;
        public string ToStoreCode { get; set; } = null!;
    }
}
