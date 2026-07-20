using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IGoodsReceivedNoteService
    {
        // Validates the PO exists and has at least one outstanding-balance line; returns
        // receivable lines joined with live stock context in one round trip.
        Task<GrnPoLookupResultServiceModel> GetReceivableLinesByPoAsync(string purchaseOrderNumber);

        // Commits a complete GRN atomically.
        Task<bool> CommitGoodsReceivedNoteAsync(
            GrnHeaderServiceModel header,
            List<GrnLineItemServiceModel> lines,
            string username);

        // Lists every PO for this buyer+order that still has at least one line with an
        // outstanding balance to receive. Powers a "pick a PO from a list" dropdown as
        // an alternate entry point to typing the PO number directly.
        Task<List<GrnPendingPoServiceModel>> GetPendingPosByOrderAsync(int buyerCode, string order);
    }
}
