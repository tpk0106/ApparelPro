using apparelPro.BusinessLogic.Services.Models.OrderManagement.IMonthlyActualShipmentsReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IMonthlyActualShipmentsReportService
    {
        Task<MonthlyActualShipmentsReportServiceModel> GetMonthlyActualShipmentsReportAsync(int month, int year);
    }
}
