using apparelPro.BusinessLogic.Services.Models.OrderManagement.IPurchaseOrderListReportService;

namespace apparelPro.BusinessLogic.Services.interfaces.Reports.OrderManagement
{
    public interface IPurchaseOrderListReportService
    {
        Task<PurchaseOrderListReportServiceModel> GetPurchaseOrderListReportAsync(string purchaseOrderNumber);

        // Backs the frontend's Purchase Order Autocomplete - legacy used a masked
        // free-text input with F1 help; this app's established selector pattern is a
        // dropdown, so this feeds that instead.
        Task<List<string>> GetPurchaseOrderNumbersAsync();
    }
}
