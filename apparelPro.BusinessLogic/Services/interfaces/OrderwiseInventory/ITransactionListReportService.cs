using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;

namespace apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory
{
    public interface ITransactionListReportService
    {
        Task<TransactionListReportHeaderServiceModel> GetHeaderAsync(DateOnly fromDate, DateOnly toDate, string? transactionType, string? itemCodePrefix);
        Task<List<TransactionListReportLineServiceModel>> GetLinesAsync(DateOnly fromDate, DateOnly toDate, string? transactionType, string? itemCodePrefix);
    }
}
