using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStyleDetailsService
    {
        Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //   Task<PaginationResult<CountryServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? filter, string? sortColumn, bool? descending);

        Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByPageNumberAsync(int pageNumber, int pageSize);

        Task<IEnumerable<StyleDetailsServiceModel>> FilterStyleDetailsByCodeAsync(string filter, int pageNumber, int pageSize);
        //Task<StyleDetailsServiceModel> GetStyleDetailsByBuyerOrderTypeStyleAsync(int buyer, string order, int type, string style);
        Task<StyleDetailsServiceModel> GetStyleDetailsByBuyerOrderTypeStyleAsync(int buyer, string order, int type, string style);
        Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderAsync(int buyer, string order, int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderAsync(int buyer, string order);
        Task<bool> DoesStyleDetailsExistAsync(string code);
        Task<StyleDetailsServiceModel> AddStyleDetailsAsync(CreateStyleDetailsServiceModel createCountryServiceModel);
        Task UpdateStyleDetailsAsync(UpdateStyleDetailsServiceModel updateCountryServiceModel);
        Task DeleteStyleDetailsAsync(int buyer, string order, int type, string style);
    }
}
