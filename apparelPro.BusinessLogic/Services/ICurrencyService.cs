using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICurrencyService
    {
        Task<PaginationResult<CurrencyServiceModel>> GetCurrenciesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //Task<IEnumerable<CurrencyServiceModel>> GetCurrenciesAsync();
        Task<CurrencyServiceModel> GetCurrencyByCodeAsync(string code);
        Task<CurrencyServiceModel> AddCurrencyAsync(CreateCurrencyServiceModel createCurrencyServiceModel);
        Task UpdateCurrencyAsync(UpdateCurrencyServiceModel updateCurrencyServiceModel);
        Task DeleteCurrencyAsync(string code);
        Task<bool> DoesCurrencyExistAsync(string code, string countryCode);
    }
}
