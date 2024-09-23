
using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class BankService : IBankService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
        public BankService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
            ILookupConstants lookupConstants, IDistributedCache distributedCache)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProReferenceDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }

        public async Task<PaginationResult<BankServiceModel>> GetBanksAsync(int pageNumber, int pageSize, string? sortColumn, 
            string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Bank> BankPagination = _apparelProDbContext.Banks.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Bank));
                BankPagination = BankPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await BankPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                BankPagination = BankPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));                
            }

            List<Bank>? result = null;

            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<Bank>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                BankPagination = BankPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await BankPagination.ToListAsync();

                _distributedCache.Set(cacheKey, result, _options);
            }

            var filteredDbCountries = result; 
            var BankServiceModels = _mapper.Map<IList<BankServiceModel>>(filteredDbCountries);

            return new PaginationResult<BankServiceModel>(pageSize, pageNumber, counter, BankServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public Task<BankServiceModel> AddBankAsync(CreateBankServiceModel createBankServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBankAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesBankExistAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BankServiceModel>> FilterBanksByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<BankServiceModel> GetBankByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<BankServiceModel> GetBankByIdAsync(string code)
        {
            var bankDbModel = await _apparelProDbContext.Banks
                .Where(bank => bank.BankCode == code)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            var bankServiceModel = _mapper.Map<BankServiceModel>(bankDbModel);
            return bankServiceModel;
        }

        public async Task<IEnumerable<BankServiceModel>> GetBanksAsync()
        {
            var bankDbModels = await _apparelProDbContext.Banks.ToListAsync();
            var bankServiceModels = _mapper.Map<IEnumerable<BankServiceModel>>(bankDbModels);
            return bankServiceModels;
        }       

        public Task<IEnumerable<BankServiceModel>> GetBanksByPageNumberAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBankAsync(UpdateBankServiceModel updateBankServiceModel)
        {
            throw new NotImplementedException();
        }
    }
}
