using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IColorSizeReportService
    {
        Task<ColorSizeReportServiceModel> GetColorSizeReportAsync(int buyerCode, string order);
    }
}
