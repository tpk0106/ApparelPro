namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRtnPrintHeaderServiceModel
    {
        public string RtnNumber { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
        public string DepartmentName { get; set; } = "";
        public string StoreCode { get; set; } = null!;
        public string StoreDescription { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
