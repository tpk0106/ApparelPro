using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyOverviewReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionSummaryMonthlyOverviewReportService
    {
        Task<ProductionSummaryMonthlyOverviewReportServiceModel> GetReportAsync(int year, int month);
    }
}
