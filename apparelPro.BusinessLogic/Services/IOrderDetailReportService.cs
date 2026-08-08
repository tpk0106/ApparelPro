using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderDetailReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IOrderDetailReportService
    {
        // Scoped to Buyer+Order only (Type/Style intentionally not part of this report's
        // scope - see OrderDetailReportServiceModel's class-level SCOPE NOTE).
        Task<OrderDetailReportServiceModel> GetOrderDetailReportAsync(int buyerCode, string order);
    }
}
