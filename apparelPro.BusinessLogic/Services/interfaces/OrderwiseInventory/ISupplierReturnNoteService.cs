using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface ISupplierReturnNoteService
    {
        // Validates the Buyer/Order exists (mirrors legacy "seek xbuyer+xorder" on od_po),
        // then returns every stock row under it that currently has QtyInHand > 0 — powers
        // the item picker. Never trusted as authoritative by the commit below.
        Task<List<SrnReturnableStockRowServiceModel>> GetReturnableStockByBuyerOrderAsync(
            int buyerCode, string order);

        // Commits a complete Supplier Return Note atomically.
        Task<bool> CommitSupplierReturnNoteAsync(
            SrnHeaderServiceModel header,
            List<SrnLineItemServiceModel> lines,
            string username);
    }
}
