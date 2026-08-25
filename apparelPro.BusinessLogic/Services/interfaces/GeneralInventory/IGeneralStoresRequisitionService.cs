using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStoresRequisitionService
    {
        Task<GeneralStockItemAvailabilityServiceModel> VerifyStockItemAvailabilityAsync(string storeCode, string itemCode, string targetUnit);

        Task<bool> CommitGeneralStoresRequisitionNoteAsync(GeneralRequisitionHeaderServiceModel header, List<GeneralRequisitionLineItemServiceModel> lines, string username);

        Task<List<GeneralStockLookupRowServiceModel>> GetAvailableStockChoicesAsync(string storeCode);

        Task<GeneralStrnPrintDetailsServiceModel> GetGeneralStrnPrintDetailsAsync(string srnNumber);

        Task<List<GeneralStoreServiceModel>> GetStoresAsync();
    }
}
