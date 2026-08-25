using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralGoodsTransferService
    {
        Task<bool> CommitGeneralGoodsTransferNoteAsync(
            GeneralGtnHeaderServiceModel header,
            List<GeneralGtnLineItemServiceModel> lines,
            string username);

        Task<GeneralGtnPrintDetailsServiceModel> GetGeneralGtnPrintDetailsAsync(string gtnNumber);
    }
}
