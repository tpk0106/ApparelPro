using apparelPro.BusinessLogic.Services.Models.Production.IManpowerRequirementReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IManpowerRequirementReportService
    {
        Task<ManpowerRequirementReportServiceModel> GetReportAsync(
            int buyerCode, string order, int typeCode, string styleCode, string? lineCode);
    }
}
