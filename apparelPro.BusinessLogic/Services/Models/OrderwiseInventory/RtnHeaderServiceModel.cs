namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RtnHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Server-assigned during commit via DocumentSequences (NoteType "RTN") — never
        // trusted from client input, same convention as RequisitionHeaderServiceModel.StrnNumber.
        public string RtnNumber { get; set; } = null!;
    }
}
