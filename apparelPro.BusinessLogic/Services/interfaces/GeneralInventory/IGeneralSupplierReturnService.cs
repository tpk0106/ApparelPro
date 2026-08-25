using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralSupplierReturnService
    {
        Task<bool> CommitGeneralSupplierReturnNoteAsync(
            GeneralSrtnHeaderServiceModel header,
            List<GeneralSrtnLineItemServiceModel> lines,
            string username);

        Task<GeneralSrtnPrintDetailsServiceModel> GetGeneralSrtnPrintDetailsAsync(string srtnNumber);
    }
}
