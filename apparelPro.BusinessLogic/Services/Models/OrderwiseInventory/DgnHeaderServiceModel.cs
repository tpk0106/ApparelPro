namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DgnHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Server-assigned during commit via DocumentSequences (NoteType "DGN") — never
        // trusted from client input, same convention as SrnHeaderServiceModel.SrnNumber.
        public string DgnNumber { get; set; } = null!;
    }
}
