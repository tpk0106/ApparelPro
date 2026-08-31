using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockValuationMonthlyReportService
    {
        Task<StockValuationMonthlyReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate);
        Task<List<StockValuationMonthlyReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate);
    }
}
