namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class RtnHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Deliberately NOT included: an RtnNumber field. It's server-assigned during
        // commit via DocumentSequences (NoteType "RTN") and returned in the top-level
        // RtnNumber field of the commit response — never part of the request. Adding a
        // non-nullable string RtnNumber here previously caused ASP.NET Core's implicit
        // required-field validation (from nullable reference types + [ApiController]) to
        // reject every request with "The RtnNumber field is required", since the
        // frontend never sends one. Same convention as GrnHeaderAPIModel, which also
        // has no allocated-number field on the request side.
    }
}
