namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGinHeaderServiceModel
    {
        public string GinNumber { get; set; } = null!;
        public string SourceStrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
