using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IGoodsIssueService
    {
        // Validates the STRN exists and has outstanding balance; returns issuable lines
        // joined with live stock context in one round trip.
        Task<GinStrnLookupResultServiceModel> GetIssuableStrnLinesAsync(string strnNumber);

        // Commits a complete GIN atomically. isManagerOverrideAuthorized must be computed
        // by the caller from the authenticated user's role — never trust a client-supplied
        // claim of authorization.
        Task<bool> CommitGoodsIssueNoteAsync(
            GinHeaderServiceModel header,
            List<GinLineItemServiceModel> lines,
            string username,
            bool isManagerOverrideAuthorized,
            bool overrideExactConsumptionCheck);

        // Lists every STRN for this buyer+order that still has at least one line with an
        // outstanding balance to receive. Powers a "pick an STRN from a list" dropdown as
        // an alternate entry point to typing the STRN number directly — purely additive,
        // does not change GetIssuableStrnLinesAsync's contract or behavior.
        Task<List<GinPendingStrnServiceModel>> GetPendingStrnsByOrderAsync(int buyerCode, string order);
    }
}
