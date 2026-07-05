
using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using QuestPDF.Helpers;
using System.Diagnostics.Metrics;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class BankService : IBankService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        //private readonly IDbContextFactory<ApparelProDbContext> dbContextFactory;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;

        //private static readonly __mapper = private new Mapper();
        public BankService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
            ILookupConstants lookupConstants, IDistributedCache distributedCache)
        {
            _mapper = mapper;
           _apparelProDbContext = apparelProReferenceDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
            //__mapper = new Mapper();
        }


        //private readonly static  Func<ApparelProDbContext,int,int,string,string,string,string, Task<PaginationResult<BankServiceModel>>> GetAllBanksAsync = 
        //    EF.CompileAsyncQuery(async (ApparelProDbContext   _apparelProDbContext, int pageNumber, int pageSize, string? sortColumn,
        //    string? sortOrder, string? filterColumn, string? filterQuery) =>
        //    {
        //        IQueryable<Bank> BankPagination = _apparelProDbContext.Banks.AsNoTracking();

        //        FilterResult fr = new();
        //        fr.searchPattern = "{0}.Contains(@0)";
        //        fr.FilterColumn = filterColumn;
        //        fr.FilterQuery = filterQuery;
        //        if (filterColumn != null && filterQuery != null)
        //        {
        //            fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Bank));
        //            BankPagination = BankPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
        //        }

        //        int counter = 0;
        //        counter = await BankPagination.CountAsync();

        //        if (sortColumn != null)
        //        {
        //            sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
        //            BankPagination = BankPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
        //        }

        //        List<Bank>? result = null;

        //        BankPagination = BankPagination
        //            .Skip(pageSize * pageNumber)
        //            .Take(pageSize);

        //        result = await BankPagination.ToListAsync();

        //        var filteredDbCountries = result;
        //        var BankServiceModels = _mapper.Map<IList<BankServiceModel>>(filteredDbCountries);                

        //        return new PaginationResult<BankServiceModel>(pageSize, pageNumber, counter, BankServiceModels,
        //            sortColumn, sortOrder, filterColumn, filterQuery);
        //    }            
        // );

        //public async Task<PaginationResult<BankServiceModel>> GetBanksStaticAsync(int pageNumber, int pageSize, string? sortColumn,
        //string? sortOrder, string? filterColumn, string? filterQuery)
        //{
        //    return await GetAllBanksAsync(_apparelProDbContext, pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        //}

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

            //var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            //var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            //_distributedCache.TryGetValue<List<Bank>>(cacheKey, out result);

            //if (await _distributedCache.GetAsync(cacheKey) == null)
            //{
            //    BankPagination = BankPagination
            //        .Skip(pageSize * pageNumber)
            //        .Take(pageSize);

            //    result = await BankPagination.ToListAsync();

            //    _distributedCache.Set(cacheKey, result, _options);
            //}

            BankPagination = BankPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            result = await BankPagination.ToListAsync();

            var filteredDbCountries = result;
            var BankServiceModels = _mapper.Map<IList<BankServiceModel>>(filteredDbCountries);

            return new PaginationResult<BankServiceModel>(pageSize, pageNumber, counter, BankServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<BankServiceModel> AddBankAsync(CreateBankServiceModel createBankServiceModel)
        {
            // 1. AutoMapper converts DTO into our strong relational Database Entity
            // This implicitly maps the collection arrays and applies the Guid generator rules
            var bankDbModel = _mapper.Map<Bank>(createBankServiceModel);

            // 2. Explicitly bind the parent lookup reference token to each child address row
            if (bankDbModel.Addresses != null && bankDbModel.Addresses.Any())
            {
                foreach (var address in bankDbModel.Addresses)
                {
                    address.BankCode = bankDbModel.BankCode; // Enforces the database relational glue
                    address.Default = address.Default; // Preserves primary branch boolean flags
                }
            }

            // 3. Track the parent container. EF handles the nested inserts out of the box!
            _apparelProDbContext.Banks.Add(bankDbModel);

            // 4. Fire a single atomic SaveChanges call to commit everything to SQL Server
            await _apparelProDbContext.SaveChangesAsync();

            // 5. Return the newly created relational graph schema
            return _mapper.Map<BankServiceModel>(bankDbModel);            
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

        //public Task<BankServiceModel> GetBankByCodeAsync(string code)
        //{
        //    IQueryable<Bank> countries = _apparelProDbContext.Countries;
        //    var bankDbModel = await countries.Where(Bank => Bank.Code == code)
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync();


        //    //var bankDbModel = await _apparelProDbContext.Countries
        //    //    .Where(Bank => Bank.Code == code)
        //    //    .AsNoTracking()
        //    //    .FirstOrDefaultAsync();
        //    var countryServiceModel = _mapper.Map<CountryServiceModel>(bankDbModel);
        //    return countryServiceModel;
        //}

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

        public async Task UpdateBankAsync(UpdateBankServiceModel updateBankServiceModel)
        {
            var bankDbModel = await _apparelProDbContext.Banks
             .Where(bank => bank.BankCode == updateBankServiceModel.BankCode)
             .FirstOrDefaultAsync();

            bankDbModel!.Name = updateBankServiceModel.Name;
            bankDbModel.CurrencyCode = updateBankServiceModel.CurrencyCode;
            //bankDbModel.AddressId = updateBankServiceModel.AddressId;            
            bankDbModel.LoanLimit = updateBankServiceModel.LoanLimit;
            bankDbModel.SwiftCode = updateBankServiceModel.SwiftCode;
            bankDbModel.TelephoneNos = updateBankServiceModel.TelephoneNos;
            
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<BankServiceModel> GetBankByBankCodeAsync(string code)
        {
            IQueryable<Bank> banks = _apparelProDbContext.Banks;
            var bankDbModel = await banks.Where(Bank => Bank.BankCode == code).Include(bank=>bank.Addresses)
                .AsNoTracking()
                .FirstOrDefaultAsync();
           
            var bankServiceModel = _mapper.Map<BankServiceModel>(bankDbModel);
            return bankServiceModel;
        }
    }
}
