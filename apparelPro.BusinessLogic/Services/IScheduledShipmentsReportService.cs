using apparelPro.BusinessLogic.Services.Models.OrderManagement.IScheduledShipmentsReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IScheduledShipmentsReportService
    {
        Task<ScheduledShipmentsReportServiceModel> GetScheduledShipmentsReportAsync(int? buyerCode, string? order);
    }
}
