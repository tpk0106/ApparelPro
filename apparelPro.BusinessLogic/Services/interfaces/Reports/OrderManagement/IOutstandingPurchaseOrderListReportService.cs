using apparelPro.BusinessLogic.Services.Models.OrderManagement.IOutstandingPurchaseOrderListReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement
{
    public interface IOutstandingPurchaseOrderListReportService
    {
        Task<OutstandingPurchaseOrderListReportServiceModel> GetOutstandingPurchaseOrderListReportAsync(
            DateOnly startDate, DateOnly endDate, string? basisCode);
    }
}
