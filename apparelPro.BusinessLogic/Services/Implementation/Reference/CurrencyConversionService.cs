using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;

        public CurrencyConversionService(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apparelProDbContext = apparelProDbContext ?? throw new ArgumentNullException(nameof(apparelProDbContext));
            _lookupConstants = lookupConstants ?? throw new ArgumentNullException(nameof(lookupConstants));
        }

        public async Task<PaginationResult<CurrencyConversionServiceModel>> GetCurrencyConversionsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<CurrencyConversion> currencyConversionPagination = _apparelProDbContext.CurrencyConversions.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(CurrencyConversion));
                currencyConversionPagination = currencyConversionPagination
                    .Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await currencyConversionPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                currencyConversionPagination = currencyConversionPagination
                    .OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            // NOTE: the previous version of this method cached the raw entity page in
            // IDistributedCache under a "currency-conversion:..." key. That cache was never
            // invalidated anywhere (Add/Update/Delete didn't exist yet to invalidate it), so
            // once this screen goes live a stale page would keep being served for up to 30
            // seconds after every save - acceptable for a low-traffic reference table, but not
            // worth the added complexity for a table this small. Removed; every request reads
            // straight from the database, same as Stock/Order Items Catalog/Basis do.
            currencyConversionPagination = currencyConversionPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var result = await currencyConversionPagination.ToListAsync();
            var currencyConversionServiceModels = _mapper.Map<IList<CurrencyConversionServiceModel>>(result);

            await AttachCurrencyNamesAsync(currencyConversionServiceModels);

            return new PaginationResult<CurrencyConversionServiceModel>(pageSize, pageNumber, counter, currencyConversionServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<CurrencyConversionServiceModel?> GetCurrencyConversionByFromToAsync(string fromCurrency, string toCurrency)
        {
            var from = (fromCurrency ?? string.Empty).Trim().ToUpperInvariant();
            var to = (toCurrency ?? string.Empty).Trim().ToUpperInvariant();

            var currencyConversionDbModel = await _apparelProDbContext.CurrencyConversions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FromCurrency.ToUpper() == from && c.ToCurrency.ToUpper() == to);

            if (currencyConversionDbModel == null)
                return null;

            var currencyConversionServiceModel = _mapper.Map<CurrencyConversionServiceModel>(currencyConversionDbModel);
            await AttachCurrencyNamesAsync(new List<CurrencyConversionServiceModel> { currencyConversionServiceModel });
            return currencyConversionServiceModel;
        }

        public async Task<CurrencyConversionServiceModel> AddCurrencyConversionAsync(CreateCurrencyConversionServiceModel createCurrencyConversionServiceModel)
        {
            var from = (createCurrencyConversionServiceModel.FromCurrency ?? string.Empty).Trim().ToUpperInvariant();
            var to = (createCurrencyConversionServiceModel.ToCurrency ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                throw new InvalidOperationException("From Currency and To Currency are both required.");

            if (from == to)
                throw new InvalidOperationException("From Currency and To Currency must be different.");

            // Both codes must exist in the Currency master - same guard Order Items Catalog
            // applies against Stocks before inserting a StockItems row.
            var fromExists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == from);
            if (!fromExists)
                throw new InvalidOperationException($"From Currency '{from}' was not found in the Currency master. Add it on the Currency reference screen first.");

            var toExists = await _apparelProDbContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == to);
            if (!toExists)
                throw new InvalidOperationException($"To Currency '{to}' was not found in the Currency master. Add it on the Currency reference screen first.");

            var alreadyExists = await _apparelProDbContext.CurrencyConversions
                .AsNoTracking()
                .AnyAsync(c => c.FromCurrency == from && c.ToCurrency == to);
            if (alreadyExists)
                throw new InvalidOperationException($"A conversion rate from {from} to {to} already exists.");

            var currencyConversionDbModel = new CurrencyConversion
            {
                FromCurrency = from,
                ToCurrency = to,
                Value = createCurrencyConversionServiceModel.Value,
            };

            _apparelProDbContext.CurrencyConversions.Add(currencyConversionDbModel);
            await _apparelProDbContext.SaveChangesAsync();

            var currencyConversionServiceModel = _mapper.Map<CurrencyConversionServiceModel>(currencyConversionDbModel);
            await AttachCurrencyNamesAsync(new List<CurrencyConversionServiceModel> { currencyConversionServiceModel });
            return currencyConversionServiceModel;
        }

        public async Task UpdateCurrencyConversionAsync(UpdateCurrencyConversionServiceModel updateCurrencyConversionServiceModel)
        {
            var from = (updateCurrencyConversionServiceModel.FromCurrency ?? string.Empty).Trim().ToUpperInvariant();
            var to = (updateCurrencyConversionServiceModel.ToCurrency ?? string.Empty).Trim().ToUpperInvariant();

            var currencyConversionDbModel = await _apparelProDbContext.CurrencyConversions
                .FirstOrDefaultAsync(c => c.FromCurrency == from && c.ToCurrency == to);

            if (currencyConversionDbModel == null)
                throw new InvalidOperationException($"No conversion rate found from {from} to {to}.");

            currencyConversionDbModel.Value = updateCurrencyConversionServiceModel.Value;
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeleteCurrencyConversionAsync(string fromCurrency, string toCurrency)
        {
            var from = (fromCurrency ?? string.Empty).Trim().ToUpperInvariant();
            var to = (toCurrency ?? string.Empty).Trim().ToUpperInvariant();

            var currencyConversionDbModel = await _apparelProDbContext.CurrencyConversions
                .FirstOrDefaultAsync(c => c.FromCurrency == from && c.ToCurrency == to);

            if (currencyConversionDbModel == null)
                return;

            _apparelProDbContext.CurrencyConversions.Remove(currencyConversionDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }

        // NEW (2026-08-07) - see the interface comment. Trims/uppercases both currency codes
        // so lookups are resilient to the same casing/whitespace inconsistencies the rest of
        // this codebase already guards against (od_conv-style flat rate table: one row per
        // From/To pair, Value is the multiplier - matches CurrencyConversion's shape exactly).
        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            var from = (fromCurrency ?? string.Empty).Trim().ToUpperInvariant();
            var to = (toCurrency ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || from == to)
                return amount;

            var rate = await _apparelProDbContext.CurrencyConversions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FromCurrency.ToUpper() == from && c.ToCurrency.ToUpper() == to);

            if (rate == null)
            {
                // Per explicit project decision (2026-08-07): block with a clear error rather
                // than silently passing the amount through unconverted or guessing a rate -
                // a wrong Trim Sheet total is worse than a report that fails to generate.
                throw new InvalidOperationException(
                    $"No currency conversion rate found from {from} to {to}. Add one on the Currency Conversion reference screen before generating this report.");
            }

            return amount * rate.Value;
        }

        // Bulk-looks-up FromCurrency/ToCurrency display names from the Currency master in a
        // single query, same pattern OrderItemCatalogService uses for Stock descriptions -
        // avoids one Currencies round-trip per row.
        private async Task AttachCurrencyNamesAsync(IList<CurrencyConversionServiceModel> currencyConversions)
        {
            if (currencyConversions.Count == 0) return;

            var codes = currencyConversions
                .SelectMany(c => new[] { c.FromCurrency, c.ToCurrency })
                .Distinct()
                .ToList();

            var nameLookup = await _apparelProDbContext.Currencies
                .AsNoTracking()
                .Where(c => codes.Contains(c.Code))
                .ToDictionaryAsync(c => c.Code, c => c.Name);

            foreach (var conversion in currencyConversions)
            {
                conversion.FromCurrencyName = nameLookup.TryGetValue(conversion.FromCurrency, out var fromName) ? fromName : null;
                conversion.ToCurrencyName = nameLookup.TryGetValue(conversion.ToCurrency, out var toName) ? toName : null;
            }
        }
    }
}
