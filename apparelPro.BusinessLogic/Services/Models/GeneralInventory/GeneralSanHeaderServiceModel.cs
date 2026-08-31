namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralSanHeaderServiceModel
    {
        public string SanNumber { get; set; } = null!; // allocated by the C# backend (NoteType "GSAN")
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
