using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseDetailedReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionSummaryStyleWiseDetailedReportService
    {
        Task<ProductionSummaryStyleWiseDetailedReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate);
    }
}
