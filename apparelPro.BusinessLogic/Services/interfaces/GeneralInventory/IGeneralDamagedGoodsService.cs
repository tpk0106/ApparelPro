using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralDamagedGoodsService
    {
        Task<bool> CommitGeneralDamagedGoodsNoteAsync(
            GeneralDgnHeaderServiceModel header,
            List<GeneralDgnLineItemServiceModel> lines,
            string username);

        Task<GeneralDgnPrintDetailsServiceModel> GetGeneralDgnPrintDetailsAsync(string dgnNumber);
    }
}
