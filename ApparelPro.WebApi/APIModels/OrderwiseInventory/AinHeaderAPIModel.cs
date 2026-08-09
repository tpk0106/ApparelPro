namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string SubContractorCode { get; set; } = null!;
        public string AdditionalProcessCode { get; set; } = null!;

        // Deliberately NOT included: an AinNumber field. It's server-assigned during commit
        // via DocumentSequences (NoteType "AIN") and returned in the top-level AinNumber
        // field of the commit response - never part of the request. Same convention as
        // every other note type's header API model.
    }
}
