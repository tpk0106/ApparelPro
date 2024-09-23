using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;
using apparelPro.BusinessLogic.Services.Models.Registration.IUserService;

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
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }
        public Task<BuyerServiceModel> AddbuyerAsync(CreateUserServiceModel createbuyerServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeletebuyerAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BuyerServiceModel>> FilterCountriesByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginationResult<BuyerServiceModel>> GetBuyersAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            //    IQueryable<Buyer> buyerPagination = _apparelProDbContext.Buyers
            //        .AsNoTracking()
            //        .Include(address =>address.Addresses)
            //        .AsNoTracking();

            var buyerPagination = _apparelProDbContext.Buyers
                .AsNoTracking()
                .Join(_apparelProDbContext.Addresses,
                    buyer => buyer.AddressId,
                    address => address.AddressId,
                    (buyer, address) => new { buyer, address })
                .AsNoTracking();
            //.Select(result =>
            //         new Buyer
            //         {
            //             BuyerCode = result.buyer.BuyerCode,
            //             Name = result.buyer.Name,
            //             Status = result.buyer.Status,
            //             TelephoneNos = result.buyer.TelephoneNos,
            //             MobileNos = result.buyer.MobileNos,
            //             CUSDEC = result.buyer.CUSDEC,
            //             Fax = result.buyer.Fax,
            //             AddressId = result.address.AddressId,
            //             Addresses = new List<Address> { result.address }
            //         }
            //);

            var mylist = new List<Buyer>();
            foreach(var e in buyerPagination)
            {
                var y = e;
                Console.WriteLine(e.buyer);
                mylist.Add(e.buyer);
            }


            buyerPagination = (from buyer in _apparelProDbContext.Buyers
                               join address in _apparelProDbContext.Addresses
                               on buyer.AddressId equals address.AddressId
                               into joined

                               from address in joined.DefaultIfEmpty()
                               select new { buyer, address });
            //select new Buyer
            //{
            //    Addresses = (ICollection<Address>)buyer.Addresses.
            //    BuyerCode = buyer.BuyerCode,
            //    AddressId = buyer.AddressId,
            //    CUSDEC = buyer.CUSDEC,
            //    Fax = buyer.Fax,
            //    MobileNos = buyer.MobileNos,
            //    Name = buyer.Name,
            //    Status = buyer.Status,
            //    TelephoneNos = buyer.TelephoneNos,
            //});
            //buyerPagination.Select(r => new Buyer
            //{
            //     Addresses = r.buyer.Addresses,
            //    BuyerCode = r.buyer.BuyerCode,
            //    AddressId = r.buyer.AddressId,
            //    CUSDEC = r.buyer.CUSDEC,
            //    Fax = r.buyer.Fax,
            //    MobileNos = r.buyer.MobileNos,
            //    Name = r.buyer.Name,
            //    Status = r.buyer.Status,
            //    TelephoneNos = r.buyer.TelephoneNos,
            //});

            buyerPagination.Select(r => new BuyerResult
            {
                Buyer = r.buyer, 
                Addresses = (List<Address>)r.buyer.Addresses!               
                
            });
            var buyerList = new List<Buyer>();
            foreach (var e in buyerPagination)
            {
                var y = e;
                Console.WriteLine(e);
                buyerList.Add(e.buyer);
            }

            //  buyerPagination = buyerList
            var buyerPagination1 = _apparelProDbContext.Buyers
                .AsNoTracking()
                .GroupJoin(_apparelProDbContext.Addresses,
                    buyer => buyer.AddressId,
                    address => address.AddressId,
                    (buyer, address) => new { buyer, address })
                .SelectMany(x => x.address.DefaultIfEmpty(),
                (buyer, address) => new { buyer,address }).AsEnumerable();

            var list = buyerPagination1.ToList();

            var buyers = list.Select(r => new Buyer
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

           // buyerPagination =   buyers.AsQueryable()

      //buyerPagination1.Select(r => new Buyer
      //      {
      //          BuyerCode = r.buyer.buyer.BuyerCode,
      //           AddressId = r.buyer.buyer.AddressId,
      //            CUSDEC = r.buyer.buyer.CUSDEC,
      //             Fax = r.buyer.buyer.Fax,
      //              MobileNos = r.buyer.buyer.MobileNos,
      //           Name =    r.buyer.buyer.Name,
      //            Status = r.buyer.buyer.Status,
      //             TelephoneNos = r.buyer.buyer.TelephoneNos,
      //          Addresses = (ICollection<Address>)r.buyer.address
      //      });
            //.Select(result =>
            //         new Buyer
            //         {
            //             BuyerCode = result.buyer.buyer.BuyerCode,
            //             Name = result.buyer.buyer.Name,
            //             Status = result.buyer.buyer.Status,
            //             TelephoneNos = result.buyer.buyer.TelephoneNos,
            //             MobileNos = result.buyer.buyer.MobileNos,
            //             CUSDEC = result.buyer.buyer.CUSDEC,
            //             Fax = result.buyer.buyer.Fax,
            //             Addresses = (ICollection<Address>)result.address!
            //         }
            //);


            //mylist = new List<Buyer>();
            //foreach (var e in buyerPagination1)
            //{
            //    var y = e;
            //    Console.WriteLine(e.buyer);
            //    mylist.Add(e.buyer.buyer);
            //}

            //  var list = buyerPagination.ToList();
            //buyerPagination = buyerPagination
            //   .Join(_apparelProDbContext.Addresses,
            //       Buyer => Buyer.AddressId,
            //       address => address.AddressId,
            //       (buyer, address) => new { buyer, address })
            //   .AsNoTracking()
            //   .Where(joined => joined.buyer.AddressId == joined.address.AddressId)
            //   .Select(result =>
            //       new Buyer
            //       {
            //           BuyerCode = result.buyer.BuyerCode,
            //           Name = result.buyer.Name,
            //           Status = result.buyer.Status,
            //           TelephoneNos = result.buyer.TelephoneNos,
            //           MobileNos = result.buyer.MobileNos,
            //           CUSDEC = result.buyer.CUSDEC,
            //           Fax = result.buyer.Fax,
            //           AddressId = result.address.AddressId,
            //           Addresses = new List<Address> { result.address }
            //       }
            //   );

            //buyerPagination = buyerPagination
            //    .Join(_apparelProDbContext.Addresses,
            //        Buyer => Buyer.AddressId,
            //        address => address.AddressId,
            //        (buyer, address) => new { buyer, address })
            //    .AsNoTracking()
            //    .Where(joined => joined.buyer.AddressId == joined.address.AddressId)
            //    .Select(result =>
            //        new Buyer
            //        {
            //            BuyerCode = result.buyer.BuyerCode,
            //            Name = result.buyer.Name,
            //            Status = result.buyer.Status,
            //            TelephoneNos = result.buyer.TelephoneNos,
            //            MobileNos = result.buyer.MobileNos,
            //            CUSDEC = result.buyer.CUSDEC,
            //            Fax = result.buyer.Fax,
            //            AddressId = result.address.AddressId,
            //            Addresses = new List<Address> { result.address }
            //        }
            //    );

            //buyerPagination = buyerPagination
            //    .Join(_apparelProDbContext.Addresses,
            //        Buyer => Buyer.AddressId,
            //        address => address.AddressId,
            //        (buyer, address) => new { buyer, address })
            //    .AsNoTracking()
            //    .Where(joined => joined.buyer.AddressId == joined.address.AddressId)
            //    .GroupBy(group => new { group.buyer.AddressId }).
            //.Select(result =>
            //    new Buyer
            //    {
            //        BuyerCode = result.buyer.BuyerCode,
            //        Name = result.buyer.Name,
            //        Status = result.buyer.Status,
            //        TelephoneNos = result.buyer.TelephoneNos,
            //        MobileNos = result.buyer.MobileNos,
            //        CUSDEC = result.buyer.CUSDEC,
            //        Fax = result.buyer.Fax,
            //        AddressId = result.address.AddressId,
            //        Addresses = new List<Address> { result.address }
            //    }
            // );

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
            //var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            //_distributedCache.TryGetValue<List<Buyer>>(cacheKey, out result);

            //if (await _distributedCache.GetAsync(cacheKey) == null)
            //{
            //    buyerPagination = buyerPagination
            //        .Skip(pageSize * pageNumber)
            //    .Take(pageSize);

            //    result = await buyerPagination.ToListAsync();

            //    _distributedCache.Set(cacheKey, result, _options);
            //}

            // var list = _apparelProDbContext.Addresses.Join(result,a => a.AddressId,b=>b.AddressId,(r) => new {r})
           // var filteredDbCountries = await buyerPagination.ToListAsync();
            var filteredDbCountries = buyers.ToList();
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
             (buyer, address) => new { buyer, address });

            var addresses = buyerPagination.Select(r => r.address).ToList();
            //var addresses = buyerPagination.Select(r => r.address).GroupBy(g=>g.AddressId).ToList();
            //var addresses1 = buyerPagination.GroupBy(g => g.address).Select(x=>x.Key);
            
            //   .GroupBy(g => new { g.buyer, g.address }).Select(x => x.Key.buyer).ToList();

            //var xx = buyerPagination.DistinctBy(x => x.address).ToList();
            var xxy = buyerPagination.Distinct().Select(x=>x.buyer.buyer);
            var xxy1 = buyerPagination.Distinct().Select(x=>x.buyer).Distinct();


            var buyers = buyerPagination.DistinctBy(x=>x.buyer.buyer.BuyerCode)
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

            }).ToList();


            //  var both = buyerPagination.Select(b=>b.buyer).ToList();

            //   var list =  buyerPagination.ToList();

            //var buyers = list.Select(r => new Buyer
            //{
            //BuyerCode = r.buyer.buyer.BuyerCode,
            //    AddressId = r.buyer.buyer.AddressId,
            //    CUSDEC = r.buyer.buyer.CUSDEC,
            //    Fax = r.buyer.buyer.Fax,
            //    MobileNos = r.buyer.buyer.MobileNos,
            //    Name = r.buyer.buyer.Name,
            //    Status = r.buyer.buyer.Status,
            //    TelephoneNos = r.buyer.buyer.TelephoneNos,
            //    Addresses = r.buyer.buyer.Addresses
            //});
            var filteredDbCountries =  buyers;
            var buyerServiceModels = _mapper.Map<IEnumerable<BuyerServiceModel>>(buyers);

            return buyerServiceModels;
        }

        //public async Task<IEnumerable<BuyerServiceModel>> GetBuyersAsync()
        //{
        //    var buyerDbModels =  await _apparelProDbContext.Buyers.ToListAsync();
        //    var buyerServiceModels = _mapper.Map<IEnumerable<BuyerServiceModel>>(buyerDbModels);
        //    return buyerServiceModels;
        //}

        public Task<IEnumerable<BuyerServiceModel>> GetCountriesByPageNumberAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<BuyerServiceModel> GetbuyerByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task UpdatebuyerAsync(UpdateBuyerServiceModel updatebuyerServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task<BuyerServiceModel> GetCountryByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<BuyerServiceModel> AddCountryAsync(CreateUserServiceModel createCountryServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCountryAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<BuyerServiceModel> GetBuyerByBuyerCodeAsync(int buyerCode)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BuyerServiceModel>> FilterBuyerByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<BuyerServiceModel> AddBuyerAsync(CreateBuyerServiceModel createBuyerServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBuyerAsync(string code)
        {
            throw new NotImplementedException();
        }

        
    }
}
