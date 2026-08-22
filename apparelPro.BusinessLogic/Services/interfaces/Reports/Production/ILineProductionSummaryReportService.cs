using apparelPro.BusinessLogic.Services.Models.Production.ILineProductionSummaryReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface ILineProductionSummaryReportService
    {
        Task<LineProductionSummaryReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate);
    }
}
