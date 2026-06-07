using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Xml.Schema;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class BuyerService : IBuyerService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public BuyerService(IMapper mapper, ApparelProDbContext apparelProDbContext, 
            ILookupConstants lookupConstants,IDistributedCache distributedCache )
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
            if (distributedCache == null)
            {
                throw new ArgumentNullException(nameof(distributedCache));
            }
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }

        public async Task<PaginationResult<BuyerServiceModel>> GetBuyersAsync(int pageNumber, int pageSize, string? sortColumn, 
            string? sortOrder, string? filterColumn, string? filterQuery)
        {
            //var buyerPagination1 = _apparelProDbContext.Buyers
            //.AsNoTracking()
            //.GroupJoin(_apparelProDbContext.Addresses,
            //    buyer => buyer.AddressId,
            //    address => address.AddressId,
            //    (buyer, address) => new { buyer, address })
            //.SelectMany(x => x.address.DefaultIfEmpty(),
            //   (buyer, address) => new { buyer, address })
            //.AsSplitQuery();

            //var addresses = buyerPagination1
            //    .Select(r => r.address)
            //    .ToList();


           var addressesWithCountries = _apparelProDbContext!.Addresses
                .Join(_apparelProDbContext.Countries,
                address => address.CountryCode,
                country => country.Code,
                (address, country) => new { address, country }).AsQueryable();

            var buyerPagination = _apparelProDbContext.Buyers
             .AsNoTracking()            
             .Select(r => new Buyer
             {
                 BuyerCode = r.BuyerCode,
                 AddressId = r.AddressId,
                 CUSDEC = r.CUSDEC,
                 Fax = r.Fax,
                 MobileNos = r.MobileNos,
                 Name = r.Name,
                 Status = r.Status,
                 TelephoneNos = r.TelephoneNos,               
                 //Addresses = _apparelProDbContext!.Addresses.Where(a =>a.AddressId == r.AddressId).ToList(),               
                 Addresses = addressesWithCountries
                    .Where(a =>a.address.AddressId == r.AddressId)
                    .Select(JoinedAddressCountry => new Address{ 
                        Id = JoinedAddressCountry.address.Id,
                        AddressId = JoinedAddressCountry.address.AddressId, 
                        AddressType = JoinedAddressCountry.address.AddressType, 
                        BuyerCode = JoinedAddressCountry.address.BuyerCode, 
                        City = JoinedAddressCountry.address.City, 
                        Country = JoinedAddressCountry.country.Name, 
                        CountryCode = JoinedAddressCountry.address.CountryCode, 
                        Default = JoinedAddressCountry.address.Default, 
                        PostCode = JoinedAddressCountry.address.PostCode, 
                        State = JoinedAddressCountry.address.State, 
                        StreetAddress = JoinedAddressCountry.address.StreetAddress  
                    })
                    .ToList(),
             }).AsQueryable();


            //var buyers = buyerPagination1 //.DistinctBy(x => x.buyer.buyer.BuyerCode)
            //                             // .GroupBy(g=> new { g.buyer, g.address })
            // .Select(r => new Buyer
            // {
            //     BuyerCode = r.buyer.buyer.BuyerCode,
            //     AddressId = r.buyer.buyer.AddressId,
            //     CUSDEC = r.buyer.buyer.CUSDEC,
            //     Fax = r.buyer.buyer.Fax,
            //     MobileNos = r.buyer.buyer.MobileNos,
            //     Name = r.buyer.buyer.Name,
            //     Status = r.buyer.buyer.Status,
            //     TelephoneNos = r.buyer.buyer.TelephoneNos,
            //   //  Addresses = null,
            //     Addresses = addresses.Where(a => a!.AddressId == r.buyer.buyer.AddressId)!,
            // });

           // var list = await buyers.ToListAsync();
         //   buyerPagination =  buyerPagination.AsQueryable();
        //    var list = await buyerPagination.ToListAsync();

            //list = _apparelProDbContext.Buyers
            //  .AsNoTracking()
            //  .Join(_apparelProDbContext.Addresses,
            //     buyer => buyer.AddressId,
            //     address => address.AddressId,
            //     (buyer, address) => new { buyer, address })
            //  .Where(x => x.buyer.AddressId == x.address.AddressId).ToList();
            //  .Select(r => new { buer = r.buyer, addresses = r.address }).ToList();
            //  .SelectMany(args => new { args.buyer, args.address })


            //.GroupBy(grp => new { grp.address.AddressId })
            //.Select(grp =>
            //new Buyer
            //{
            //    BuyerCode = grp,
            //    AddressId = buyer.buyer.AddressId,
            //    CUSDEC = buyer.buyer.CUSDEC,
            //    Fax = buyer.buyer.Fax,
            //    MobileNos = buyer.buyer.MobileNos,
            //    Name = buyer.buyer.Name,
            //    Status = buyer.buyer.Status,
            //    TelephoneNos = buyer.buyer.TelephoneNos,
            //    Addresses = buyer.address

            //}).AsQueryable();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Buyer));
                buyerPagination = buyerPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }          

            int counter = 0;
            counter = await buyerPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                buyerPagination = buyerPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));               
            }

            //List<Buyer>? result = null;

            //var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            //var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 2) };

            //_distributedCache.TryGetValue<List<Buyer>>(cacheKey, out result);

            //if (await _distributedCache.GetAsync(cacheKey) == null)
            //{
            //    buyerPagination = buyerPagination
            //        .Skip(pageSize * pageNumber)
            //        .Take(pageSize);

            //    result = await buyerPagination.ToListAsync();

            //    _distributedCache.Set(cacheKey, result, _options);
            //}

            buyerPagination = buyerPagination
                   .Skip(pageSize * pageNumber)
                   .Take(pageSize);

            var filteredDbCountries = await buyerPagination.ToListAsync();           
            var buyerServiceModels = _mapper.Map<IList<BuyerServiceModel>>(filteredDbCountries);

            return new PaginationResult<BuyerServiceModel>(pageSize, pageNumber, counter, buyerServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<IEnumerable<BuyerServiceModel>> GetBuyersExAsync()
        {
            var buyerPagination = _apparelProDbContext.Buyers
             .AsNoTracking()
             .GroupJoin(_apparelProDbContext.Addresses,
                 buyer => buyer.AddressId,
                 address => address.AddressId,
                 (buyer, address) => new { buyer, address })
             .SelectMany(x => x.address.DefaultIfEmpty(),
                (buyer, address) => new { buyer, address })
             .AsSplitQuery();

            var addresses = buyerPagination
                .Select(r => r.address)
                .ToList();
            //var addresses = buyerPagination.Select(r => r.address).GroupBy(g=>g.AddressId).ToList();
            //var addresses1 = buyerPagination.GroupBy(g => g.address).Select(x=>x.Key);            
            //   .GroupBy(g => new { g.buyer, g.address }).Select(x => x.Key.buyer).ToList();
            //var xx = buyerPagination.DistinctBy(x => x.address).ToList();
            var xxy = buyerPagination.Distinct().Select(x=>x.buyer.buyer);
            var xxy1 = buyerPagination.Distinct().Select(x=>x.buyer).Distinct();

            var buyers = buyerPagination//.DistinctBy(x => x.buyer.buyer.BuyerCode)
                // .GroupBy(g=> new { g.buyer, g.address })
                .Select(r => new Buyer
                {
                    BuyerCode = r.buyer.buyer.BuyerCode,
                    AddressId = r.buyer.buyer.AddressId,
                    CUSDEC = r.buyer.buyer.CUSDEC,
                    Fax = r.buyer.buyer.Fax,
                    MobileNos = r.buyer.buyer.MobileNos,
                    Name = r.buyer.buyer.Name,
                    Status = r.buyer.buyer.Status,
                    TelephoneNos = r.buyer.buyer.TelephoneNos,
                    Addresses = r.buyer.buyer.Addresses

                });

            var buyerPagination2 = _apparelProDbContext.Buyers
                .AsNoTracking()
                .Include(address => address.Addresses)
                .AsNoTracking();

            buyers = buyerPagination2.Select(r => new Buyer
            {
                BuyerCode = r.BuyerCode,
                AddressId = r.AddressId,
                CUSDEC = r.CUSDEC,
                Fax = r.Fax,
                MobileNos = r.MobileNos,
                Name = r.Name,
                Status = r.Status,
                TelephoneNos = r.TelephoneNos,
                Addresses = r.Addresses
            });

            var filteredDbCountries =  buyers;
            var buyerServiceModels = _mapper.Map<IEnumerable<BuyerServiceModel>>(buyers);

            return buyerServiceModels;
        }      

        public async Task<BuyerServiceModel> GetBuyerByBuyerCodeAsync(int buyerCode)
        {
            var buyerDbModel = await _apparelProDbContext.Buyers
                .Where(buyer => buyer.BuyerCode == buyerCode)
                .FirstOrDefaultAsync();
            var buyerServiceModel = _mapper.Map<BuyerServiceModel>(buyerDbModel);
            return buyerServiceModel;
        }

        public async Task<BuyerServiceModel> AddBuyerAsync(CreateBuyerServiceModel createBuyerServiceModel)
        {
            var addressId = Guid.NewGuid();
           createBuyerServiceModel.AddressId = addressId.ToString();
           //get addressid from addresses 
            var buyerDbModel = _mapper.Map<Buyer>(createBuyerServiceModel);
            _apparelProDbContext.Buyers.Add(buyerDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<BuyerServiceModel>(buyerDbModel);
        }

        public async Task DeleteBuyerAsync(int buyerCode)
        {
            var buyerDbModel = await _apparelProDbContext.Buyers
               .Where(buyer => buyer.BuyerCode == buyerCode)
               .FirstOrDefaultAsync();
            _apparelProDbContext.Buyers.Remove(buyerDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task UpdateBuyerAsync(UpdateBuyerServiceModel updateBuyerServiceModel)
        {
            var BuyerDbModel = await _apparelProDbContext.Buyers
              .Where(Buyer => Buyer.BuyerCode == updateBuyerServiceModel.BuyerCode)
              .FirstOrDefaultAsync();

            BuyerDbModel!.Name = updateBuyerServiceModel.Name!;
            BuyerDbModel.TelephoneNos = updateBuyerServiceModel.TelephoneNos;
            BuyerDbModel.MobileNos = updateBuyerServiceModel.MobileNos;
            BuyerDbModel.CUSDEC = updateBuyerServiceModel.CUSDEC;
            BuyerDbModel.Fax = updateBuyerServiceModel.Fax;
            BuyerDbModel.Status = updateBuyerServiceModel.Status!;
            _apparelProDbContext.Update(BuyerDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }      

        public Task<IEnumerable<BuyerServiceModel>> FilterBuyerByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }       
    }
}
