using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IStockStatusReportService
    {
        Task<StockStatusReportHeaderServiceModel> GetHeaderAsync(int buyerCode, string order);
        Task<List<StockStatusReportLineServiceModel>> GetLinesAsync(int buyerCode, string order);
    }
}
