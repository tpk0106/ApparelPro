using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralPurchaseOrderListReportService
    {
        Task<GeneralPurchaseOrderListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate);
        Task<List<GeneralPurchaseOrderListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate);
    }
}
