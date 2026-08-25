namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinHeaderAPIModel
    {
        public string GinNumber { get; set; } = null!;
        public string SourceStrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
