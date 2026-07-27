using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockAdjustmentNoteService
    {
        // Validates the Buyer/Order exists, then returns EVERY stock row under it —
        // no QtyInHand filter, since a stock take must be able to correct a count from
        // zero as well as to zero. Powers the item picker.
        Task<List<SanAdjustableStockRowServiceModel>> GetAdjustableStockByBuyerOrderAsync(
            int buyerCode, string order);

        // Commits a complete Stock Adjustment Note atomically. Unlike every other note
        // type, this SETS OrderwiseStock.QtyInHand to the entered value rather than
        // adding/subtracting a movement.
        Task<bool> CommitStockAdjustmentNoteAsync(
            SanHeaderServiceModel header,
            List<SanLineItemServiceModel> lines,
            string username);
    }
}
