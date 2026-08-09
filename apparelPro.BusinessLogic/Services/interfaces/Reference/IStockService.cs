using apparelPro.BusinessLogic.Services.Models.Reference.IStockService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    public interface IStockService
    {
        Task<PaginationResult<StockServiceModel>> GetStocksAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<StockServiceModel?> GetStockByCodeAsync(string stockCode);
        Task<bool> DoesStockExistAsync(string stockCode);
        Task<StockServiceModel> AddStockAsync(CreateStockServiceModel createStockServiceModel);
        Task UpdateStockAsync(UpdateStockServiceModel updateStockServiceModel);
        Task DeleteStockAsync(string stockCode);
    }
}
