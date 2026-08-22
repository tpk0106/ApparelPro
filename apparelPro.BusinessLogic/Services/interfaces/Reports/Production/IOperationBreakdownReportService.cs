using apparelPro.BusinessLogic.Services.Models.Production.IOperationBreakdownReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IOperationBreakdownReportService
    {
        Task<OperationBreakdownReportServiceModel> GetReportAsync(int buyerCode, string order, int typeCode, string styleCode);
    }
}
