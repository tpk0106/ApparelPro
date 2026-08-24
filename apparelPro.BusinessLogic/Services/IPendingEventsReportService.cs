using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPendingEventsReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IPendingEventsReportService
    {
        Task<PendingEventsReportServiceModel> GetPendingEventsReportAsync(DateTime asOfDate);
    }
}
