using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryMonthlyReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionSummaryMonthlyReportService
    {
        Task<ProductionSummaryMonthlyReportServiceModel> GetReportAsync(int year, int month);
    }
}
