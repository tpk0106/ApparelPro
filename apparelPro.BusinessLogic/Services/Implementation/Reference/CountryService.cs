
using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class CountryService : ICountryService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public CountryService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
            ILookupConstants lookupConstants, IDistributedCache distributedCache)
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
        public async Task<CountryServiceModel> AddCountryAsync(CreateCountryServiceModel createCountryServiceModel)
        {
            var countryDbModel = _mapper.Map<Country>(createCountryServiceModel);
            _apparelProDbContext.Countries.Add(countryDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<CountryServiceModel>(countryDbModel);
        }

        public async Task DeleteCountryAsync(string code)
        {
            var countryDbModel = await _apparelProDbContext.Countries
               .Where(country => country.Code == code)
               .FirstOrDefaultAsync();
            _apparelProDbContext.Countries.Remove(countryDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<PaginationResult<CountryServiceModel>> GetCountriesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {            
            IQueryable<Country> countryPagination = _apparelProDbContext.Countries.AsNoTracking();
  
            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Country));
                countryPagination = countryPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }          

            int counter = 0;
            counter = await countryPagination.CountAsync();
            
            if(sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                countryPagination = countryPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
                //sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                //countryPagination = sortOrder.ToUpper() == "ASC" ?
                //    countryPagination.OrderByColumn(sortColumn.Trim()) :
                //    countryPagination.OrderByColumnDescending(sortColumn.Trim());
            }

            List<Country>? result = null;
           
            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<Country>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                countryPagination = countryPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await countryPagination.ToListAsync();              

                _distributedCache.Set(cacheKey, result, _options);             
            }

            var filteredDbCountries = result; 
            var countryServiceModels = _mapper.Map<IList<CountryServiceModel>>(filteredDbCountries);

            return new PaginationResult<CountryServiceModel>(pageSize, pageNumber, counter, countryServiceModels,
                sortColumn,sortOrder,filterColumn,filterQuery);        
        }

        public async Task<IEnumerable<CountryServiceModel>> GetCountriesByPageNumberAsync(int pageNumber, int pageSize)
        {
            IQueryable<Country> countryPagination = _apparelProDbContext.Countries.TagWith("GetCountriesByPageNumberAsync").AsNoTracking();
            
            int counter = await countryPagination.CountAsync();

            countryPagination = countryPagination
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize);                

            var filteredCountries = await countryPagination.OrderBy(c=>c.Code).ToListAsync();            
            
            var countryServiceModels = _mapper.Map<IEnumerable<CountryServiceModel>>(filteredCountries);
            return countryServiceModels;
        }

        public async Task<IEnumerable<CountryServiceModel>> FilterCountriesByCodeAsync(string? filter, int pageNumber, int pageSize)
        {
            IQueryable<Country> countryPagination = _apparelProDbContext.Countries.TagWith("FilterCountriesByCodeAsync").AsNoTracking();

            int counter = await countryPagination.CountAsync();

            if(filter != null)
            {
                countryPagination = countryPagination.Where(country => country.Code.Contains(filter));
            }

            countryPagination = countryPagination             
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize);

            var filteredCountries = await countryPagination.OrderBy(c => c.Code).ToListAsync();

            var countryServiceModels = _mapper.Map<IEnumerable<CountryServiceModel>>(filteredCountries);
            return countryServiceModels;
        }

        public async Task<CountryServiceModel> GetCountryByCodeAsync(string code)
        {
            //1 
            //var countryDbModel =  await _apparelProDbContext.Countries.AsQueryable().Where(country => country.Code == code).AsNoTracking().ToListAsync();
            
            // 2
            IQueryable<Country> countries = _apparelProDbContext.Countries;
            var countryDbModel = await countries.Where(country => country.Code == code)
                .AsNoTracking()
                .FirstOrDefaultAsync();


            //var countryDbModel = await _apparelProDbContext.Countries
            //    .Where(country => country.Code == code)
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync();
            var countryServiceModel = _mapper.Map<CountryServiceModel>(countryDbModel);
            return countryServiceModel;
        }

        public async Task UpdateCountryAsync(UpdateCountryServiceModel updateCountryServiceModel)
        {
           //var countryDbModel = _mapper.Map<Country>(updateCountryServiceModel);
           var countryDbModel = await _apparelProDbContext.Countries
               .Where(country => country.Code == updateCountryServiceModel.Code)
               .FirstOrDefaultAsync();

            countryDbModel!.Name = updateCountryServiceModel.Name;
            countryDbModel.Flag = updateCountryServiceModel.Flag;
            countryDbModel.Code = updateCountryServiceModel.Code;
            await _apparelProDbContext.SaveChangesAsync();

            //_apparelProDbContext.Countries.Update(countryDbModel!);
            //await IdentityHelpers.SaveChangesWithIdentityInsertAsync<Country>(_apparelProDbContext);            
        }

        public async Task<bool> DoesCountryExistAsync(string code)
        {
            IQueryable<Country> countries = _apparelProDbContext.Countries;
            var countryDbModel = await countries.Where(country => country.Code == code)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return countryDbModel != null;
        }
    }
}
