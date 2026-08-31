using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockMovementReportService
    {
        Task<GeneralStockMovementReportHeaderServiceModel> GetHeaderAsync(string storeCode, string itemCode, int month, int year);
        Task<List<GeneralStockMovementReportLineServiceModel>> GetLinesAsync(string storeCode, string itemCode, int month, int year);
    }
}
