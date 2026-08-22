using apparelPro.BusinessLogic.Services.Models.Production.ILineEfficiencyReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface ILineEfficiencyReportService
    {
        Task<LineEfficiencyReportServiceModel> GetReportAsync(string lineCode, int year, int month);
    }
}
