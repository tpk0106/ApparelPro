namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SanHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Server-assigned during commit via DocumentSequences (NoteType "SAN") — never
        // trusted from client input, same convention as every other note type.
        public string SanNumber { get; set; } = null!;
    }
}
