using apparelPro.BusinessLogic.Services.Models.Production.IProductionScheduleReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionScheduleReportService
    {
        Task<ProductionScheduleReportServiceModel> GetProductionScheduleReportAsync(DateOnly fromDate, DateOnly toDate);
    }
}
