namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralDgnHeaderAPIModel
    {
        public string DgnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
