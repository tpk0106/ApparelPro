using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralPurchaseOrderService
    {
        Task<GeneralPoCommitResultServiceModel> CommitGeneralPurchaseOrderAsync(
            GeneralPoHeaderServiceModel header,
            List<GeneralPoLineItemServiceModel> lines,
            string username);

        Task<GeneralPOServiceModel> GetGeneralPurchaseOrderAsync(string poNumber);

        Task<GeneralPoPrintDetailsServiceModel> GetGeneralPoPrintDetailsAsync(string poNumber);
    }
}
