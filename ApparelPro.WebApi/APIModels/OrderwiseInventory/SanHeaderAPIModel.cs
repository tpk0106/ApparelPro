namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Deliberately NOT included: a SanNumber field. It's server-assigned during commit
        // via DocumentSequences (NoteType "SAN") and returned in the top-level SanNumber
        // field of the commit response — never part of the request. Same convention as
        // every other note type's header API model.
    }
}
