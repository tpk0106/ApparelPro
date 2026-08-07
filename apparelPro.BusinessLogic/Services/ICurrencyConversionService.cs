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

        // NEW (2026-08-07) - the actual "convert an amount between two currencies" utility
        // legacy's curconv() provided, which nothing in this service previously implemented
        // (every method above only manages the CurrencyConversions rate table itself, never
        // uses it to convert a value). Added for the Trim Sheet Report, which needs to convert
        // each material line's own currency into the order's currency before totalling, exactly
        // like curconv() does in OD_TRIM.PRG. Same-currency pairs pass through unconverted;
        // amount * CurrencyConversions.Value applies the trim-migration-project-team's stated
        // rule that a missing rate should hard-fail rather than silently produce a wrong total.
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
