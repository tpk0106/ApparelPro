using apparelPro.BusinessLogic.Services.Models.Production.IDailyEmployeeEfficiencyReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IDailyEmployeeEfficiencyReportService
    {
        Task<DailyEmployeeEfficiencyReportServiceModel> GetReportAsync(DateOnly date, string? lineCode);
    }
}
