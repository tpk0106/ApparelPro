using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICurrencyExchangeService
    {    
        Task<PaginationResult<CurrencyExchangeServiceModel>> GetCurrencyExchangesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        //Task<IEnumerable<CurrencyExchangeServiceModel>> GetCurrencyExchangesAsync();
        Task<IEnumerable<CurrencyExchangeServiceModel>> GetCurrencyExchangesByDateAsync();
        Task<IEnumerable<CurrencyExchangeServiceModel>> GetCurrencyExchangesByBaseCurrencyAsync(string baseCurrency);
        Task<CurrencyExchangeServiceModel> GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(string baseCurrency, string quoteCurrency, DateTime date);
        Task<CurrencyExchangeServiceModel> AddCurrencyExchangeAsync(CreateCurrencyExchangeServiceModel createCurrencyExchangeServiceModel );
        Task UpdateCurrencyExchangeAsync(UpdateCurrencyExchangeServiceModel updateCurrencyExchangeServiceModel);
        Task DeleteCurrencyExchangeAsync(string baseCurrency, string quoteCurrency, DateOnly exchangeDate);
    }
}
