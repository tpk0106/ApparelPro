using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Shared
{
    public class AddressService : IAddressService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public AddressService(IMapper mapper, ApparelProDbContext apparelProReferenceDbContext,
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

        public async Task<AddressServiceModel> AddAddressAsync(CreateAddressServiceModel createAddressServiceModel)
        {
            var AddressDbModel = _mapper.Map<Address>(createAddressServiceModel);
            _apparelProDbContext.Addresses.Add(AddressDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<AddressServiceModel>(AddressDbModel);
        }

        public Task DeleteAddressAsync(string code)
        {
            throw new NotImplementedException();
        }
     
        public async Task<PaginationResult<AddressServiceModel>> GetAddressesAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {

            var filteredAddressAndCountryAndBuyerJoined = _apparelProDbContext.Addresses
                .AsNoTracking()
                .Join(_apparelProDbContext.Countries,
                    address => address.CountryCode,
                    country => country.Code,
                    (address, country) => new { address, country })
                .AsNoTracking()
                .Join(_apparelProDbContext.Buyers,
                    (addressAndCountry) => addressAndCountry.address.BuyerCode, buyer => buyer.BuyerCode, (addressAndCountry,buyer)=> 
                new Address
                {
                    AddressId = addressAndCountry.address.AddressId,
                    AddressType = addressAndCountry.address.AddressType,                 
                    BuyerCode =buyer.BuyerCode,
                    City = addressAndCountry.address.City,
                    Default = addressAndCountry.address.Default,
                    PostCode = addressAndCountry.address.PostCode,
                    State = addressAndCountry.address.State,
                    StreetAddress = addressAndCountry.address.StreetAddress,
                    Country = addressAndCountry.country.Name,
                    CountryCode = addressAndCountry.country.Code,
                    Id = addressAndCountry.address.Id
                }).AsNoTracking();                        

         var addressPagination = filteredAddressAndCountryAndBuyerJoined.AsQueryable();

          FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Address));
                addressPagination = addressPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await addressPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                addressPagination = addressPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));               
            }

            List<Address>? result = null;

            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<Address>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                addressPagination = addressPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);         

                result =  await addressPagination.ToListAsync() ;
                var _AddressServiceModels = _mapper.Map<IList<AddressServiceModel>>(result);

                _distributedCache.Set(cacheKey, _AddressServiceModels, _options);
            }

            var filteredDbAddresses= result; // await AddressPagination.ToListAsync();
            var addressServiceModels = _mapper.Map<IList<AddressServiceModel>>(filteredDbAddresses);

            return new PaginationResult<AddressServiceModel>(pageSize, pageNumber, counter, addressServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }      

        public async Task<PaginationResult<AddressServiceModel>> GetAddressesByAddresIdAsync
        (
            Guid addressId, 
            int pageNumber, 
            int pageSize, 
            string? sortColumn, 
            string? sortOrder, 
            string? filterColumn, 
            string? filterQuery
        )
        {
            var addressPagination = _apparelProDbContext.Addresses
                .Where(address => address.AddressId == addressId)
                .AsNoTracking()
                .AsQueryable();              

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Address));
                addressPagination = addressPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await addressPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                addressPagination = addressPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }        

            var filteredDbAddresses = await addressPagination.ToListAsync();
            var addressServiceModels = _mapper.Map<IList<AddressServiceModel>>(filteredDbAddresses);

            return new PaginationResult<AddressServiceModel>(pageSize, pageNumber, counter, addressServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);            
        }       

        public async Task UpdateAddressAsync(UpdateAddressServiceModel updateAddressServiceModel)
        {
            var addressDbModel = _mapper.Map<Address>(updateAddressServiceModel);
            _apparelProDbContext.Addresses.Update(addressDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }      

        public async Task<AddressServiceModel> GetAddressByIdAndAddresIdAsync(int id, Guid addressId)
        {
            var addresDbModel = await _apparelProDbContext.Addresses
               .Where(address => address.Id == id && address.AddressId == addressId)
               .AsNoTracking()
               .FirstOrDefaultAsync();

            var addresSrviceModel = _mapper.Map<AddressServiceModel>(addresDbModel);
            return addresSrviceModel;
        }
        public Task<bool> DoesAddressExistAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AddressServiceModel>> FilterAddressesByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAddressAsync(int id, Guid addressId)
        {
            throw new NotImplementedException();
        }
    }
}
