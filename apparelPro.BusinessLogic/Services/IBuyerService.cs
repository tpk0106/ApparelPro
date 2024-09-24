using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IBuyerService
    {
        Task<PaginationResult<BuyerServiceModel>> GetBuyersAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);        
        Task<IEnumerable<BuyerServiceModel>> GetBuyersExAsync();        
        Task<BuyerServiceModel> GetBuyerByBuyerCodeAsync(int buyerCode);
        Task<IEnumerable<BuyerServiceModel>> FilterBuyerByCodeAsync(string filter, int pageNumber, int pageSize);       
        Task<BuyerServiceModel> AddBuyerAsync(CreateBuyerServiceModel createBuyerServiceModel);
        Task UpdateBuyerAsync(UpdateBuyerServiceModel updateCountryServiceModel);
        Task DeleteBuyerAsync(int buyerCode);
    }
}
