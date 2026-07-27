namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DgnHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Deliberately NOT included: a DgnNumber field. It's server-assigned during commit
        // via DocumentSequences (NoteType "DGN") and returned in the top-level DgnNumber
        // field of the commit response — never part of the request. Same convention as
        // SrnHeaderAPIModel/GtnHeaderAPIModel/RtnHeaderAPIModel.
    }
}
