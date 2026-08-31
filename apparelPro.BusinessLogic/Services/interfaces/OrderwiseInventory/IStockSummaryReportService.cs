using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockSummaryReportService
    {
        Task<StockSummaryReportHeaderServiceModel> GetHeaderAsync(string currency1, string currency2);
        Task<List<StockSummaryReportLineServiceModel>> GetLinesAsync(string currency1, string currency2);
    }
}
