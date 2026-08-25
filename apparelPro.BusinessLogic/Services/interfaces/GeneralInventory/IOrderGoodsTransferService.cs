using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IOrderGoodsTransferService
    {
        Task<List<OrderGtnTransferableStockRowServiceModel>> GetTransferableStockAsync(
            string direction, string storeCode, int buyerCode, string order);

        Task<bool> CommitOrderGoodsTransferNoteAsync(
            OrderGtnHeaderServiceModel header,
            List<OrderGtnLineItemServiceModel> lines,
            string username);

        Task<OrderGtnPrintDetailsServiceModel> GetOrderGtnPrintDetailsAsync(string ogtnNumber);
    }
}
