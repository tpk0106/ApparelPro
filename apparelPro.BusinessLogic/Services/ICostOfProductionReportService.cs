using apparelPro.BusinessLogic.Services.Models.OrderManagement.ICostOfProductionReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICostOfProductionReportService
    {
        Task<CostOfProductionReportServiceModel> GetCostOfProductionReportAsync(int buyerCode, string order);
    }
}
