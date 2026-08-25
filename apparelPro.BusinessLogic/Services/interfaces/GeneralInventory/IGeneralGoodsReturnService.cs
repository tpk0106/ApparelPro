using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralGoodsReturnService
    {
        Task<bool> CommitGeneralGoodsReturnNoteAsync(
            GeneralRtnHeaderServiceModel header,
            List<GeneralRtnLineItemServiceModel> lines,
            string username);

        Task<GeneralRtnPrintDetailsServiceModel> GetGeneralRtnPrintDetailsAsync(string rtnNumber);
    }
}
