namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GinHeaderServiceModel
    {
        public string SourceStrnNumber { get; set; } = null!;
        public DateTime TransactionDate { get; set; }

        // Resolved server-side from the STRN's own transaction rows during commit —
        // never trusted from client input, even though the frontend will echo these
        // back from a prior GetIssuableSrnLinesAsync call.
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string DepartmentCode { get; set; } = null!;
    }
}
