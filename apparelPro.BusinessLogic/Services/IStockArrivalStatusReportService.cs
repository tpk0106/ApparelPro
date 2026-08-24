using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStockArrivalStatusReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStockArrivalStatusReportService
    {
        Task<StockArrivalStatusReportServiceModel> GetStockArrivalStatusReportAsync(int buyerCode, string order, DateTime asOfDate);
    }
}
