using apparelPro.BusinessLogic.Services.Models.Production.IEstimatedProductionScheduleReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IEstimatedProductionScheduleReportService
    {
        Task<EstimatedProductionScheduleReportServiceModel> GetReportAsync(DateOnly fromDate, DateOnly toDate);
    }
}
