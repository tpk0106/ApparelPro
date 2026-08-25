namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class GeneralDgnHeaderServiceModel
    {
        public string DgnNumber { get; set; } = null!; // allocated by the C# backend (NoteType "GDGN")
        public DateTime TransactionDate { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
