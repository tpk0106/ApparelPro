using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IDamagedGoodsNoteService
    {
        // Validates the Buyer/Order exists (mirrors legacy "seek xbuyer+xorder" on od_po),
        // then returns every stock row under it that currently has QtyInHand > 0 — powers
        // the item picker. Never trusted as authoritative by the commit below.
        Task<List<DgnDamageableStockRowServiceModel>> GetDamageableStockByBuyerOrderAsync(
            int buyerCode, string order);

        // Commits a complete Damaged Goods Note atomically.
        Task<bool> CommitDamagedGoodsNoteAsync(
            DgnHeaderServiceModel header,
            List<DgnLineItemServiceModel> lines,
            string username);
    }
}
