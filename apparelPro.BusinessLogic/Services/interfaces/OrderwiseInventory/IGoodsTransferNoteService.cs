using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IGoodsTransferNoteService
    {
        // Validates both Buyer/Orders exist (mirrors legacy "seek xfbuyer+xforder" /
        // "seek xtbuyer+xtorder" on od_po), then returns every From-side stock row that still
        // has something to transfer AND already exists on the To side too — powers the item
        // picker. Never trusted as authoritative by the commit below.
        Task<List<GtnTransferableStockRowServiceModel>> GetTransferableStockAsync(
            int fromBuyerCode, string fromOrder, int toBuyerCode, string toOrder);

        // Commits a complete Goods Transfer Note atomically.
        Task<bool> CommitGoodsTransferNoteAsync(
            GtnHeaderServiceModel header,
            List<GtnLineItemServiceModel> lines,
            string username);

        // Fetches one already-committed GTN's header + lines for on-screen preview / PDF print -
        // same pattern as StoresRequisitionService.GetStrnPrintDetailsAsync. Reads only the "6T"
        // (Transfer-Out) leg for the line list - both legs of a GTN share the same DocumentNumber,
        // ItemCode, Basis and Quantity, so reading just one avoids doubling every row.
        Task<GtnPrintDetailsServiceModel> GetGtnPrintDetailsAsync(string gtnNumber);
    }
}
