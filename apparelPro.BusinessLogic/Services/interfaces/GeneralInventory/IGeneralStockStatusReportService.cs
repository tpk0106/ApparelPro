using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockStatusReportService
    {
        Task<GeneralStockStatusReportHeaderServiceModel> GetHeaderAsync(string storeCode, int month, int year);

        Task<List<GeneralStockStatusReportLineServiceModel>> GetLinesAsync(string storeCode, int month, int year);
    }
}
