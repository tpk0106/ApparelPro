using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralGoodsIssueService
    {
        Task<GeneralGinStrnLookupResultServiceModel> GetIssuableStrnLinesAsync(string strnNumber);

        Task<bool> CommitGeneralGoodsIssueNoteAsync(
            GeneralGinHeaderServiceModel header,
            List<GeneralGinLineItemServiceModel> lines,
            string username,
            bool minStockOverrideAuthorized,
            bool overrideMinStockCheck);

        Task<GeneralGinPrintDetailsServiceModel> GetGeneralGinPrintDetailsAsync(string ginNumber);
    }
}
