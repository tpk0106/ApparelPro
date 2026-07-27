namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SrnHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int SupplierCode { get; set; }
        public DateTime TransactionDate { get; set; }

        // Deliberately NOT included: an SrnNumber field. It's server-assigned during commit
        // via DocumentSequences (NoteType "SRN") and returned in the top-level SrnNumber
        // field of the commit response — never part of the request. Same convention as
        // GtnHeaderAPIModel/RtnHeaderAPIModel.
    }
}
