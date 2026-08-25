namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGtnPrintHeaderAPIModel
    {
        public string GtnNumber { get; set; } = null!;
        public string FromStoreCode { get; set; } = null!;
        public string FromStoreDescription { get; set; } = "";
        public string ToStoreCode { get; set; } = null!;
        public string ToStoreDescription { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
