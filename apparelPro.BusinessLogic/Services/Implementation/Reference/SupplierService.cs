using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data.Models.References;
using ApparelPro.Data;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class SupplierService : ISupplierService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public SupplierService(IMapper mapper, ApparelProDbContext apparelProDbContext,
            ILookupConstants lookupConstants, IDistributedCache distributedCache)
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
        public async Task<SupplierServiceModel> AddSupplierAsync(CreateSupplierServiceModel createSupplierServiceModel)
        {
            var addressId = Guid.NewGuid();
            createSupplierServiceModel.AddressId = addressId;
            var supplierDbModel = _mapper.Map<Supplier>(createSupplierServiceModel);
            _apparelProDbContext.Suppliers.Add(supplierDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<SupplierServiceModel>(supplierDbModel);
        }

        public async Task DeleteSupplierAsync(int supplierCode)
        {
            var supplierDbModel = await _apparelProDbContext.Suppliers
             .Where(supplier => supplier.SupplierCode == supplierCode)
             .FirstOrDefaultAsync();
            _apparelProDbContext.Suppliers.Remove(supplierDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<SupplierServiceModel> GetSupplierBySupplierCodeAsync(int supplierCode)
        {
            var suppierDbModel = await _apparelProDbContext.Suppliers
                .Where(supplier=>supplier.SupplierCode == supplierCode)
                .FirstOrDefaultAsync();
            var supplierServiceModel = _mapper.Map<SupplierServiceModel>(suppierDbModel);
            return supplierServiceModel;
        }

        public async Task<PaginationResult<SupplierServiceModel>> GetSuppliersAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            //var supplierPagination = _apparelProDbContext.Suppliers
            // .AsNoTracking()
            // .Include(address => address.Addresses)
            // .AsNoTracking()
            // .Select(r => new Supplier
            // {
            //     SupplierCode = r.SupplierCode,
            //     AddressId = r.AddressId,
            //     Fax = r.Fax,
            //     MobileNos = r.MobileNos,
            //     Name = r.Name,                 
            //     TelephoneNos = r.TelephoneNos,
            //     Addresses = r.Addresses

            // }).AsQueryable();


            var addressesWithCountries = _apparelProDbContext!.Addresses
               .Join(_apparelProDbContext.Countries,
               address => address.CountryCode,
               country => country.Code,
               (address, country) => new { address, country }).AsQueryable();

            var supplierPagination = _apparelProDbContext.Suppliers
             .AsNoTracking()
             .Select(r => new Supplier
             {
                 SupplierCode = r.SupplierCode,
                 AddressId = r.AddressId,                
                 Fax = r.Fax,
                 MobileNos = r.MobileNos,
                 Name = r.Name,                 
                 TelephoneNos = r.TelephoneNos,                 
                 Addresses = addressesWithCountries
                    .Where(a => a.address.AddressId == r.AddressId)
                    .Select(JoinedAddressCountry => new Address
                    {
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

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Supplier));
                supplierPagination = supplierPagination
                    .Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await supplierPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                supplierPagination = supplierPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            supplierPagination = supplierPagination
                 .Skip(pageSize * pageNumber)
                 .Take(pageSize);

            var filteredDbSuppliers = await supplierPagination.ToListAsync();
            var supplierServiceModels = _mapper.Map<IList<SupplierServiceModel>>(filteredDbSuppliers);

            return new PaginationResult<SupplierServiceModel>(pageSize, pageNumber, counter, supplierServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateSupplierAsync(UpdateSupplierServiceModel updateSupplierServiceModel)
        {
            var supplierDbModel = await _apparelProDbContext.Suppliers
             .Where(supplier => supplier.SupplierCode == updateSupplierServiceModel.SupplierCode)
             .FirstOrDefaultAsync();

            supplierDbModel!.Name = updateSupplierServiceModel.Name;
            supplierDbModel.TelephoneNos = updateSupplierServiceModel.TelephoneNos;
            supplierDbModel.MobileNos = updateSupplierServiceModel.MobileNos;            
            supplierDbModel.Fax = updateSupplierServiceModel.Fax;
            
            _apparelProDbContext.Update(supplierDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
