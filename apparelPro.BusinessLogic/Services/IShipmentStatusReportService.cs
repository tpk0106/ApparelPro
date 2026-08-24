using apparelPro.BusinessLogic.Services.Models.OrderManagement.IShipmentStatusReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IShipmentStatusReportService
    {
        Task<ShipmentStatusReportServiceModel> GetShipmentStatusReportAsync(int buyerCode, string order);
    }
}
