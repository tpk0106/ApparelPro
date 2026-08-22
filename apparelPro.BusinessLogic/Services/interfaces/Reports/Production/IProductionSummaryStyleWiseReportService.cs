using apparelPro.BusinessLogic.Services.Models.Production.IProductionSummaryStyleWiseReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionSummaryStyleWiseReportService
    {
        Task<ProductionSummaryStyleWiseReportServiceModel> GetReportAsync(DateOnly startDate, DateOnly endDate);
    }
}
