using apparelPro.BusinessLogic.Services.Models.OrderManagement.IYearSeasonOrdersReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IYearSeasonOrdersReportService
    {
        Task<YearSeasonOrdersReportServiceModel> GetYearSeasonOrdersReportAsync(int? year, string? season);
    }
}
