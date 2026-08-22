using apparelPro.BusinessLogic.Services.Models.Production.IMonthlyEmployeeEfficiencyReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.Production
{
    public interface IMonthlyEmployeeEfficiencyReportService
    {
        Task<MonthlyEmployeeEfficiencyReportServiceModel> GetReportAsync(int year, int month);
    }
}
