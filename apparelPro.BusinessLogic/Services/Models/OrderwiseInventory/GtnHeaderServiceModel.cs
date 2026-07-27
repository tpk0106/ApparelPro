namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GtnHeaderServiceModel
    {
        public int FromBuyerCode { get; set; }
        public string FromOrder { get; set; } = null!;
        public int ToBuyerCode { get; set; }
        public string ToOrder { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Server-assigned during commit via DocumentSequences (NoteType "GTN") — never
        // trusted from client input, same convention as RtnHeaderServiceModel.RtnNumber.
        public string GtnNumber { get; set; } = null!;
    }
}
