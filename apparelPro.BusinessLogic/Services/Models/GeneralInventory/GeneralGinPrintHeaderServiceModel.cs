namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralGinPrintHeaderServiceModel
    {
        public string GinNumber { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public string DepartmentCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
