using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockValuationReportService
    {
        Task<StockValuationReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order);
        Task<List<StockValuationReportLineServiceModel>> GetLinesAsync(int buyerCode, string order);
    }
}
