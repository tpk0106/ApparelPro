using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockAdjustmentService
    {
        Task<bool> CommitGeneralStockAdjustmentNoteAsync(
            GeneralSanHeaderServiceModel header,
            List<GeneralSanLineItemServiceModel> lines,
            string username);

        Task<GeneralSanPrintDetailsServiceModel> GetGeneralSanPrintDetailsAsync(string sanNumber);
    }
}
