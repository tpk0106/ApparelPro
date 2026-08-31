using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockReorderReportService
    {
        Task<GeneralStockReorderReportHeaderServiceModel> GetHeaderAsync(string storeCode);
        Task<List<GeneralStockReorderReportLineServiceModel>> GetLinesAsync(string storeCode);
    }
}
