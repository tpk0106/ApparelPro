using apparelPro.BusinessLogic.Services.Models.Production.IProductionAnalysisSummaryReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IProductionAnalysisSummaryReportService
    {
        Task<ProductionAnalysisSummaryReportServiceModel> GetReportAsync(int buyerCode, string order, int typeCode, string styleCode);
    }
}
