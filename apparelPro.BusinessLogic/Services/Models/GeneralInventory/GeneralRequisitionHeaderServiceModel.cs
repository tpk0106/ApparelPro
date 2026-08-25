namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRequisitionHeaderServiceModel
    {
        public string SrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
