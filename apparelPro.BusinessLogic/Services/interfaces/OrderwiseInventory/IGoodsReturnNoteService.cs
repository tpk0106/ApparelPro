using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IGoodsReturnNoteService
    {
        // Validates Buyer/Order exists (mirrors legacy "seek xbuyer+xorder" on od_po),
        // then returns every stock row for that Buyer/Order with something returnable —
        // powers the item picker. Never trusted as authoritative by the commit below.
        Task<List<RtnReturnableStockRowServiceModel>> GetReturnableStockByBuyerOrderAsync(int buyerCode, string order);

        // Commits a complete Goods Return Note atomically.
        Task<bool> CommitGoodsReturnNoteAsync(
            RtnHeaderServiceModel header,
            List<RtnLineItemServiceModel> lines,
            string username);
    }
}
