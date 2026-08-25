namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralRtnHeaderServiceModel
    {
        public string RtnNumber { get; set; } = null!; // allocated by the C# backend (NoteType "GRTN")
        public DateTime TransactionDate { get; set; }
        public string DepartmentCode { get; set; } = null!; // material is returning FROM this department
        public string StoreCode { get; set; } = null!;      // TO this General store
    }
}
