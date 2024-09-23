using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IBuyerService
    {
        Task<PaginationResult<BuyerServiceModel>> GetBuyersAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //   Task<PaginationResult<CountryServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? filter, string? sortColumn, bool? descending);
      //  Task<PaginationResult<BuyerServiceModel>> GetBuyersAsync();
        Task<IEnumerable<BuyerServiceModel>> GetBuyersExAsync();
        //Task<IEnumerable<BuyerServiceModel>> GetBuyersAsync();
        //Task<IEnumerable<BuyerServiceModel>> GetBuyersByPageNumberAsync(int pageNumber, int pageSize);
        Task<BuyerServiceModel> GetBuyerByBuyerCodeAsync(int buyerCode);
        Task<IEnumerable<BuyerServiceModel>> FilterBuyerByCodeAsync(string filter, int pageNumber, int pageSize);
        Task<BuyerServiceModel> GetCountryByCodeAsync(string code);
        Task<BuyerServiceModel> AddBuyerAsync(CreateBuyerServiceModel createBuyerServiceModel);
      //  Task UpdateCountryAsync(UpdateBuyerServiceModel updateCountryServiceModel);
        Task DeleteBuyerAsync(string code);
    }
}
