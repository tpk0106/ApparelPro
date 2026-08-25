namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralGinPrintHeaderAPIModel
    {
        public string GinNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
