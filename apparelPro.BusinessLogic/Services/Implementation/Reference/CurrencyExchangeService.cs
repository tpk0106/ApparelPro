using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyExchangeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class CurrencyExchangeService:ICurrencyExchangeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public CurrencyExchangeService(IMapper mapper, ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants) 
        {
            if (apparelProDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProDbContext));
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
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;        
        }

        public async Task<PaginationResult<CurrencyExchangeServiceModel>> GetCurrencyExchangesAsync(
              int pageNumber,
              int pageSize,
              string? sortColumn,
              string? sortOrder,
              string? filterColumn,
              string? filterQuery
        )
        {
            IQueryable<CurrencyExchange> currencyExchangePagination = _apparelProDbContext.CurrencyExchanges.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0} == (@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null &&  filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(CurrencyExchange));
                currencyExchangePagination = currencyExchangePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await currencyExchangePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                currencyExchangePagination = currencyExchangePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));             
            }

            currencyExchangePagination = currencyExchangePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCurrencyExchanges = await currencyExchangePagination.ToListAsync();
            var currencyExchangesServiceModels = _mapper.Map<IList<CurrencyExchangeServiceModel>>(filteredDbCurrencyExchanges);

            return new PaginationResult<CurrencyExchangeServiceModel>(pageSize, pageNumber, counter, currencyExchangesServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<CurrencyExchangeServiceModel> AddCurrencyExchangeAsync(CreateCurrencyExchangeServiceModel createCurrencyExchangeServiceModel)
        {
            var currencyExchangeDbModel = _mapper.Map<CurrencyExchange>(createCurrencyExchangeServiceModel);
            _apparelProDbContext.CurrencyExchanges.Add(currencyExchangeDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<CurrencyExchangeServiceModel>(currencyExchangeDbModel);
        }

        public Task DeleteCurrencyExchangeAsync(string baseCurrency, string quoteCurrency, DateOnly exchangeDate)
        {
            throw new NotImplementedException();
        }       
        public async Task<CurrencyExchangeServiceModel> GetCurrencyExchangeByBaseCurrencyAndQuoteCurrencyOnDateAsync(string baseCurrency, string quoteCurrency, DateTime date)
        {
            var currencyExchangeDbModel = await _apparelProDbContext.CurrencyExchanges
                .Where(ce => (ce.BaseCurrency == baseCurrency) && (ce.QuoteCurrency == quoteCurrency) && (ce.ExchangeDate == date))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            var currencyExchangesServiceModels = _mapper.Map< CurrencyExchangeServiceModel>(currencyExchangeDbModel);
            return currencyExchangesServiceModels;
        }  

        public async Task<IEnumerable<CurrencyExchangeServiceModel>> GetCurrencyExchangesByDateAsync()
        {
            var currencyExchangeDbModels = await _apparelProDbContext.CurrencyExchanges
              .AsNoTracking()
              .GroupBy(grp=> new { grp.ExchangeDate, grp.BaseCurrency, grp.QuoteCurrency, grp.Rate })
              .Select(grp => new CurrencyExchange { 
                  ExchangeDate = grp.Key.ExchangeDate, 
                  BaseCurrency = grp.Key.BaseCurrency, 
                  QuoteCurrency = grp.Key.QuoteCurrency, 
                  Rate = grp.Key.Rate })              
              .OrderBy(ce =>ce.ExchangeDate)
              .ThenBy(ce =>ce.BaseCurrency)              
              .ToListAsync();
            var currencyExchangesServiceModels = _mapper.Map<IEnumerable<CurrencyExchangeServiceModel>>(currencyExchangeDbModels);
            return currencyExchangesServiceModels;
        }

        public async Task<IEnumerable<CurrencyExchangeServiceModel>> GetCurrencyExchangesByBaseCurrencyAsync(string baseCurrency)
        {
            var currencyExchangeDbModels = await _apparelProDbContext.CurrencyExchanges
                .Where(c => c.BaseCurrency == baseCurrency)
                .AsNoTracking()
                .OrderBy(c => c.ExchangeDate)
                .ThenBy(c =>c.QuoteCurrency)                
                .ToListAsync();
            var currencyExchangesServiceModels = _mapper.Map<IEnumerable<CurrencyExchangeServiceModel>>(currencyExchangeDbModels);
            return currencyExchangesServiceModels;
        }
        public Task UpdateCurrencyExchangeAsync(UpdateCurrencyExchangeServiceModel updateCurrencyExchangeServiceModel)
        {
            throw new NotImplementedException();
        }
    }
}
