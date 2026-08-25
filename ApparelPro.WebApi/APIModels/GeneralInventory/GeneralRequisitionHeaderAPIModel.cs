namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralRequisitionHeaderAPIModel
    {
        public string SrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
