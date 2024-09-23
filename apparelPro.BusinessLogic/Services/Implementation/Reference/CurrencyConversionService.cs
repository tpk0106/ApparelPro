using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyConversionService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
        public CurrencyConversionService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
            ILookupConstants lookupConstants, Microsoft.Extensions.Caching.Distributed.IDistributedCache distributedCache)
        {
            if (apparelProReferenceDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProReferenceDbContext));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (lookupConstants == null)
            {
                throw new ArgumentNullException(nameof(lookupConstants));
            }
            _mapper = mapper;
            _apparelProDbContext = apparelProReferenceDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
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

            int counter = 0;
            counter = await currencyConversionPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                currencyConversionPagination = currencyConversionPagination
                    .OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));                
            }

            List<CurrencyConversion>? result = null;

            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<CurrencyConversion>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                currencyConversionPagination = currencyConversionPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await currencyConversionPagination.ToListAsync();

                _distributedCache.Set(cacheKey, result, _options);
            }

            var filteredDbCountries = result; 
            var currencyConversionServiceModels = _mapper.Map<IList<CurrencyConversionServiceModel>>(filteredDbCountries);

            return new PaginationResult<CurrencyConversionServiceModel>(pageSize, pageNumber, counter, currencyConversionServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }
        public Task<CurrencyConversionServiceModel> AddCurrencyConversionAsync(CreateCurrencyConversionServiceModel createCurrencyConversionServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCurrencyConversionAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CurrencyConversionServiceModel>> FilterCountriesByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CurrencyConversionServiceModel>> GetCountriesByPageNumberAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<CurrencyConversionServiceModel> GetCurrencyConversionByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }      

        public Task UpdateCurrencyConversionAsync(UpdateCurrencyConversionServiceModel updateCurrencyConversionServiceModel)
        {
            throw new NotImplementedException();
        }       
    }
}
