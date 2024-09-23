using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICountryService
    {
        Task<PaginationResult<CountryServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
     //   Task<PaginationResult<CountryServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? filter, string? sortColumn, bool? descending);

        Task<IEnumerable<CountryServiceModel>> GetCountriesByPageNumberAsync(int pageNumber, int pageSize);

        Task<IEnumerable<CountryServiceModel>> FilterCountriesByCodeAsync(string filter, int pageNumber, int pageSize);
        Task<CountryServiceModel> GetCountryByCodeAsync(string code);
        Task<bool> DoesCountryExistAsync(string code);
        Task<CountryServiceModel> AddCountryAsync(CreateCountryServiceModel createCountryServiceModel);
        Task UpdateCountryAsync(UpdateCountryServiceModel updateCountryServiceModel);
        Task DeleteCountryAsync(string code);
        //Task<bool> DoesUnitExistAsync(string code);
    }
}
