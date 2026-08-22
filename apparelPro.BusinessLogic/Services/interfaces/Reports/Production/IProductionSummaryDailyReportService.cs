using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryDailyReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionSummaryDailyReportService
    {
        Task<ProductionSummaryDailyReportServiceModel> GetProductionSummaryDailyReportAsync(DateOnly date);
    }
}
