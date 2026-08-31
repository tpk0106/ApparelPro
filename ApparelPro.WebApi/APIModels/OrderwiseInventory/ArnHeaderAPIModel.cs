namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnHeaderAPIModel
    {
        public DateTime TransactionDate { get; set; }
        public string SubContractorCode { get; set; } = null!;
        public string? InvoiceNumber { get; set; }
        public string Currency { get; set; } = null!;

        // Deliberately NOT included: an ArnNumber field. It's server-assigned during commit
        // via DocumentSequences (NoteType "ARN") and returned in the top-level ArnNumber
        // field of the commit response - never part of the request. Same convention as
        // every other note type's header API model.
    }
}
