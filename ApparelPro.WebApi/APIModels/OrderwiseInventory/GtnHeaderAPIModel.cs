namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GtnHeaderAPIModel
    {
        public int FromBuyerCode { get; set; }
        public string FromOrder { get; set; } = null!;
        public int ToBuyerCode { get; set; }
        public string ToOrder { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Deliberately NOT included: a GtnNumber field. It's server-assigned during commit via
        // DocumentSequences (NoteType "GTN") and returned in the top-level GtnNumber field of
        // the commit response — never part of the request. Same convention as
        // RtnHeaderAPIModel/GrnHeaderAPIModel, which also have no allocated-number field on the
        // request side (avoids ASP.NET Core's implicit required-field validation rejecting the
        // request for a field the frontend never sends).
    }
}
