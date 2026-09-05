using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IDirectTransferNoteService
    {
        // Validates both Buyer/Orders exist, then returns every From-side stock row that
        // still has something to transfer - powers the source-item picker. Never trusted
        // as authoritative by the commit below.
        Task<List<DtnFromStockRowServiceModel>> GetFromStockAsync(
            int fromBuyerCode, string fromOrder, int toBuyerCode, string toOrder);

        // Validates the To Buyer/Order exists, then returns its material requirement
        // (StyleMaterialCostProfiles) as the candidate destination-item list - powers the
        // "map to this item" dropdown per line.
        Task<List<DtnToItemServiceModel>> GetToOrderItemsAsync(int toBuyerCode, string toOrder);

        // Commits a complete Direct Goods Transfer Note atomically.
        Task<bool> CommitDirectTransferNoteAsync(
            DtnHeaderServiceModel header,
            List<DtnLineItemServiceModel> lines,
            string username);

        // Fetches one already-committed DTN's header + lines for on-screen preview / PDF print -
        // same pattern as StoresRequisitionService.GetStrnPrintDetailsAsync.
        Task<DtnPrintDetailsServiceModel> GetDtnPrintDetailsAsync(string dtnNumber);
    }
}
