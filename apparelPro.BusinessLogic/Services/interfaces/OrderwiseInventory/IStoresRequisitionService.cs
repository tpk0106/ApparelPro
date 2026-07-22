using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStoresRequisitionService
    {
        // 1. Validates and pulls live net available stock quantities for a specific item column cell
        Task<StockItemAvailabilityDetails> VerifyStockItemAvailabilityAsync(int buyerCode, string order, string storeCode, string itemCode, string targetUnit);

        // 2. Commits a complete Stores Requisition Note header and detail rows array atomically
        Task<bool> CommitStoresRequisitionNoteAsync(RequisitionHeaderServiceModel header, List<RequisitionLineItemServiceModel> lines, string username);
        
        Task<List<OrderwiseStockLookupRowServiceModel>> GetAvailableStockChoicesAsync(int buyerCode, string order, string storeCode);

        // 4. Fetches one already-committed STRN's header + lines for on-screen preview / PDF print
        Task<StrnPrintDetailsServiceModel> GetStrnPrintDetailsAsync(string strnNumber);
    }
}
