using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralGrnListingReportService
    {
        Task<GeneralGrnListingReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? storeCode, string? supplierCode);
        Task<List<GeneralGrnListingReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? storeCode, string? supplierCode);
    }
}
