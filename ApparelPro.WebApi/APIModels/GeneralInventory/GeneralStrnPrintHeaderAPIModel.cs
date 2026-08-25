namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStrnPrintHeaderAPIModel
    {
        public string SrnNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
