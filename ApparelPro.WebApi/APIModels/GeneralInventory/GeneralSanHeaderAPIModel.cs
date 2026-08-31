namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralSanHeaderAPIModel
    {
        public string SanNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
