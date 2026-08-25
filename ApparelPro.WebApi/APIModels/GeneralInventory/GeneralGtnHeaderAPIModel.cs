namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGtnHeaderAPIModel
    {
        public string GtnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string FromStoreCode { get; set; } = null!;
        public string ToStoreCode { get; set; } = null!;
    }
}
