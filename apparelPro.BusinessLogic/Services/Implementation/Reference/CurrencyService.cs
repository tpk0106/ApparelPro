using ApparelPro.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ApparelPro.Shared.LookupConstants;
using ApparelPro.Data.Models.References;
using apparelPro.BusinessLogic.Services.Models.Reference.ICurrencyService;
using ApparelPro.Shared.Extensions;
using System.Linq.Dynamic.Core;
using Values = int[];

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public CurrencyService(ApparelProDbContext apparelProReferenceDbContext,
            IMapper mapper, ILookupConstants lookupConstants)
        {
            _apparelProDbContext = apparelProReferenceDbContext;
            _mapper = mapper;
            _lookupConstants = lookupConstants;
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
        }

        void LoopOverValues(Values values)
        {
            Console.WriteLine("Values in the array:");
            foreach (int val in values)
            {
                Console.WriteLine(val);
            }
        }
        public async Task<CurrencyServiceModel> AddCurrencyAsync(CreateCurrencyServiceModel createCurrencyServiceModel)
        {
            var currencyDbModel = _mapper.Map<Currency>(createCurrencyServiceModel);
         
            //_apparelProDbContext.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Currencies ON");

            _apparelProDbContext.Currencies.Add(currencyDbModel);
            await _apparelProDbContext.SaveChangesAsync();
          
         //   _apparelProDbContext.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Currencies OFF");
            return _mapper.Map<CurrencyServiceModel>(currencyDbModel);
        }

        public async Task DeleteCurrencyAsync(string code)
        {
            var currencyDbModel = await _apparelProDbContext.Currencies
                .Where(curr => curr.Code == code)
                .FirstOrDefaultAsync();
            _apparelProDbContext.Currencies.Remove(currencyDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<PaginationResult<CurrencyServiceModel>> GetCurrenciesAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Currency> currencyPagination = _apparelProDbContext.Currencies.AsNoTracking();

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery))
            {
                currencyPagination = currencyPagination.Where(string.Format("{0}.Contains(@0)", filterColumn), filterQuery);
            }

            int counter = 0;
            counter = await currencyPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                currencyPagination = currencyPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));                
            }
            currencyPagination = currencyPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCurrencies = await currencyPagination.ToListAsync();
            
            var filteredCurrencyAndCountryJoined = filteredDbCurrencies.Join(_apparelProDbContext.Countries,
                   currency => currency.CountryCode,
                   country => country.Code,
                   (currency, country) => new { currency, country })
                .Select(r => new CurrencyServiceModel()
                {
                    Code = r.currency.Code,
                    Name = r.currency.Name,
                    CountryCode = r.currency.CountryCode!,
                    Country = r.country,
                    Id = r.currency.Id,
                    Minor = r.currency.Minor,
                    CurrencyDetails = r.currency.CurrencyDetails
                });

            var currencyServiceModels = _mapper.Map<IList<CurrencyServiceModel>>(filteredCurrencyAndCountryJoined);

            return new PaginationResult<CurrencyServiceModel>(pageSize, pageNumber, counter, currencyServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }       

        public async Task<IEnumerable<CurrencyServiceModel>> GetCurrenciesAsync1()
        {
            var currencyDbModels = await _apparelProDbContext.Currencies
               .Join(_apparelProDbContext.Countries,
                   currency => currency.CountryCode,
                   country => country.Code,
                   (currency, country) => new { currency, country })
               .AsNoTracking()
               .ToListAsync();
            var results = currencyDbModels.Select(r => new CurrencyServiceModel()
            {
                Code = r.currency.Code,
                Name = r.currency.Name,
                CountryCode = r.currency.CountryCode,
                Country = r.country,
                Id = r.currency.Id,
                Minor = r.currency.Minor,
                CurrencyDetails = r.currency.CurrencyDetails
            });

            var currencyServiceModels = _mapper.Map<IEnumerable<CurrencyServiceModel>>(results);
            return currencyServiceModels;
        }

        public async Task<CurrencyServiceModel> GetCurrencyByCodeAsync(string code)
        {
            var currencyDbModel = await _apparelProDbContext.Currencies.Where(c=>c.Code == code).FirstOrDefaultAsync();
            //var currencyDbModel = await _apparelProDbContext.Currencies
            //    .Join(_apparelProDbContext.Countries,
            //        currency => currency.CountryCode,                 
            //        country => country.Code,
            //        (currency, country) => new { currency, country })
            //    .Where(currency => currency.currency.Code == code)
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync();

            //var results = new CurrencyServiceModel()
            //{
            //    Code = currencyDbModel!.currency.Code,
            //    Name = currencyDbModel!.currency.Name,
            //    Country = currencyDbModel!.country,
            //    Id = currencyDbModel!.currency.Id,
            //    Minor = currencyDbModel!.currency.Minor,
            //    CurrencyDetails = currencyDbModel!.currency.CurrencyDetails
            //};

            var currencyServiceModel = _mapper.Map<CurrencyServiceModel>(currencyDbModel);
            return currencyServiceModel;
        }

        public async Task UpdateCurrencyAsync(UpdateCurrencyServiceModel updateCurrencyServiceModel)
        {            
            var currencyDbModel = _apparelProDbContext.Currencies
                .Where(currency => currency.Code == updateCurrencyServiceModel.Code)
                .FirstOrDefault()!;
         
            currencyDbModel.Name = updateCurrencyServiceModel.Name!;
            currencyDbModel.CountryCode = updateCurrencyServiceModel.CountryCode;            
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<bool> DoesCurrencyExistAsync(string code, string countryCode)
        {
            IQueryable<Currency> currencies = _apparelProDbContext.Currencies;
            var currencyDbModel = await currencies.Where(currency => currency.Code == code || currency.CountryCode == countryCode)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return currencyDbModel != null;
        }
    }
}
