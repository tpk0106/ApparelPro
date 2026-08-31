using apparelPro.BusinessLogic.Services.Models.GeneralInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.GeneralInventory
{
    public interface IGeneralTransactionListReportService
    {
        Task<GeneralTransactionListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? transactionTypeCode, string? itemCodePrefix);
        Task<List<GeneralTransactionListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? transactionTypeCode, string? itemCodePrefix);
    }
}
