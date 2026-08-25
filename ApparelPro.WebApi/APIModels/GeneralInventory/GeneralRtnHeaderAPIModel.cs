namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralRtnHeaderAPIModel
    {
        public string RtnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string DepartmentCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
    }
}
