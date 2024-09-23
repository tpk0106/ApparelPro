using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICurrencyConversionService
    {
      //  Task<PaginationResult<CurrencyConversionServiceModel>> GetCurrencyConversionsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<PaginationResult<CurrencyConversionServiceModel>> GetCurrencyConversionsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

       

     //   Task<IEnumerable<CurrencyConversionServiceModel>> FilterCountriesByCodeAsync(string filter, int pageNumber, int pageSize);
        Task<CurrencyConversionServiceModel> GetCurrencyConversionByCodeAsync(string code);
        Task<CurrencyConversionServiceModel> AddCurrencyConversionAsync(CreateCurrencyConversionServiceModel createCurrencyConversionServiceModel);
        Task UpdateCurrencyConversionAsync(UpdateCurrencyConversionServiceModel updateCurrencyConversionServiceModel);
        Task DeleteCurrencyConversionAsync(string code);
    }
}
