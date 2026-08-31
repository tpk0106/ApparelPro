using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockValuationReportService
    {
        Task<GeneralStockValuationReportHeaderServiceModel> GetHeaderAsync(string storeCode, string fromItemCode, string toItemCode);
        Task<List<GeneralStockValuationReportLineServiceModel>> GetLinesAsync(string storeCode, string fromItemCode, string toItemCode);
    }
}
