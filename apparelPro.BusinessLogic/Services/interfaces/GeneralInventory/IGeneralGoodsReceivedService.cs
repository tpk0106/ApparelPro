using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralGoodsReceivedService
    {
        Task<GeneralGrnPoLookupResultServiceModel> GetReceivableLinesByPoAsync(string poNumber);

        Task<bool> CommitGeneralGoodsReceivedNoteAsync(
            GeneralGrnHeaderServiceModel header,
            List<GeneralGrnLineItemServiceModel> lines,
            string username,
            bool maxStockOverrideConfirmed);

        Task<GeneralGrnPrintDetailsServiceModel> GetGeneralGrnPrintDetailsAsync(string grnNumber);
    }
}
