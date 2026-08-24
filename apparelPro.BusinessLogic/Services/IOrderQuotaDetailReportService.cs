using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOrderQuotaDetailReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IOrderQuotaDetailReportService
    {
        Task<OrderQuotaDetailReportServiceModel> GetOrderQuotaDetailReportAsync(int? buyerCode, string? order);
    }
}
