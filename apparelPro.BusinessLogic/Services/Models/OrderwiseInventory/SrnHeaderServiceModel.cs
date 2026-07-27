namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class SrnHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int SupplierCode { get; set; }
        public DateTime TransactionDate { get; set; }

        // Server-assigned during commit via DocumentSequences (NoteType "SRN") — never
        // trusted from client input, same convention as GtnHeaderServiceModel.GtnNumber.
        public string SrnNumber { get; set; } = null!;
    }
}
