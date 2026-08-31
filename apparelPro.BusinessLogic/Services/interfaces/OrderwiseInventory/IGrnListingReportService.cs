using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface IGrnListingReportService
    {
        Task<GrnListingReportHeaderServiceModel> GetHeaderAsync(
            DateOnly? fromDate, DateOnly? toDate, int? buyerCode, string? order, string? storeCode, string? supplierCode);
        Task<List<GrnListingReportLineServiceModel>> GetLinesAsync(
            DateOnly? fromDate, DateOnly? toDate, int? buyerCode, string? order, string? storeCode, string? supplierCode);
    }
}
