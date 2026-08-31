using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralStockSummaryReportService
    {
        Task<GeneralStockSummaryReportHeaderServiceModel> GetHeaderAsync(int month, int year, string currency1, string currency2);
        Task<List<GeneralStockSummaryReportLineServiceModel>> GetLinesAsync(int month, int year, string currency1, string currency2);
    }
}
