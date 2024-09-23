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
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class StyleDetailsService : IStyleDetailsService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
        public StyleDetailsService(IMapper mapper,  ApparelProDbContext apparelProDbContext,
             ILookupConstants lookupConstants, IDistributedCache distributedCache)
        {            
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }

        public async Task<StyleDetailsServiceModel> GetStyleDetailsByBuyerOrderTypeStyleAsync(int buyer, string order, int type, string style)
        {
            var styleDbModel = await _apparelProDbContext.Styles
                .Where(_style => _style.BuyerCode == buyer && _style.Order == order && 
                    _style.TypeCode == type && _style.StyleCode == style)
                .FirstOrDefaultAsync();

            var styleServiceModel = _mapper.Map<StyleDetailsServiceModel>(styleDbModel);
            return styleServiceModel;



            //var joined = _apparelProDbContext.Styles
            //    .AsNoTracking()
            //    .Join(_apparelProDbContext.Buyers, style => style.BuyerCode,
            //        buyer => buyer.BuyerCode, (style, buyer) => new { style, buyer })
            //    .AsNoTracking()
            //    .Join(_apparelProDbContext.GarmentTypes, (styleAndBuyer) => styleAndBuyer.style.TypeCode,
            //        gt => gt.Id, (style, garmentType) => new { style, garmentType })
            //    .AsNoTracking()
            //    .Where(final => 
            //        final.style.style.BuyerCode == buyer &&
            //        final.style.style.Order == order && 
            //        final.style.style.TypeCode == type && 
            //        final.style.style.StyleCode == style
            //    )
            //    .Select(result => new Style
            //    {
            //         BuyerCode = result.style.buyer.BuyerCode,
            //         Buyer = result.style.buyer.Name!,
            //         Order = result.style.style.Order,
            //         TypeCode = result.style.style.TypeCode,
            //         Quantity = result.style.style.Quantity,
            //         UnitPrice = result.style.style.UnitPrice,
            //         Unit = result.style.style.Unit,
            //         OrderDate = result.style.style.OrderDate,
            //         ApprovedDate = result.style.style.ApprovedDate,
            //         CustomerReturn = result.style.style.CustomerReturn,
            //         EstimateApprovalDate = result.style.style.EstimateApprovalDate,
            //         EstimateApprovalUserName = result.style.style.EstimateApprovalUserName,
            //         ExportBalance = result.style.style.ExportBalance,
            //         Exported = result.style.style.Exported,
            //         ProductionEndDate = result.style.style.ProductionEndDate,
            //         SupplierReturn = result.style.style.SupplierReturn,
            //         StyleCode = result.style.style.StyleCode,
            //         Username = result.style.style.Username,
            //         Type = result.garmentType.TypeName
            //    });


        }


        public async Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderAsync(int buyer, string order, int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            //IQueryable<Style> StylePagination = _apparelProDbContext.Styles
            //   .Where(style => style.BuyerCode == buyer && style.Order == order);

            IQueryable<Style> StylePagination = _apparelProDbContext.Styles
              .AsNoTracking()
              .Join(_apparelProDbContext.Buyers, style => style.BuyerCode,
                  buyer => buyer.BuyerCode, (style, buyer) => new { style, buyer })
              .AsNoTracking()
              .Join(_apparelProDbContext.GarmentTypes, (styleAndBuyer) => styleAndBuyer.style.TypeCode,
                  gt => gt.Id, (style, garmentType) => new { style, garmentType })
              .AsNoTracking()
              .Where(joined => joined.style.style.BuyerCode == buyer && joined.style.style.Order == order)
             .Select(result => new Style
             {
                 Id = result.style.style.Id,
                 BuyerCode = result.style.buyer.BuyerCode,
                 Buyer = result.style.buyer.Name!,
                 Order = result.style.style.Order,
                 TypeCode = result.style.style.TypeCode,
                 Type = result.garmentType.TypeName,
                 Quantity = result.style.style.Quantity,
                 UnitPrice = result.style.style.UnitPrice,
                 Unit = result.style.style.Unit,
                 OrderDate = result.style.style.OrderDate,
                 ApprovedDate = result.style.style.ApprovedDate,
                 CustomerReturn = result.style.style.CustomerReturn,
                 EstimateApprovalDate = result.style.style.EstimateApprovalDate,
                 EstimateApprovalUserName = result.style.style.EstimateApprovalUserName,
                 ExportBalance = result.style.style.ExportBalance,
                 Exported = result.style.style.Exported,
                 ProductionEndDate = result.style.style.ProductionEndDate,
                 SupplierReturn = result.style.style.SupplierReturn,
                 StyleCode = result.style.style.StyleCode,
                 Username = result.style.style.Username
             });

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Style));
                StylePagination = StylePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await StylePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                StylePagination = StylePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            StylePagination = StylePagination
               .Skip(pageSize * pageNumber)
                   .Take(pageSize);

            // List<Style>? result = null;

            //var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            //var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            //_distributedCache.TryGetValue<List<Style>>(cacheKey, out result);

            //if (await _distributedCache.GetAsync(cacheKey) == null)
            //{
            //    StylePagination = StylePagination
            //    .Skip(pageSize * pageNumber)
            //        .Take(pageSize);

            //    result = await StylePagination.ToListAsync();

            //    _distributedCache.Set(cacheKey, result, _options);
            //}

            var filteredDbCountries =  await StylePagination.ToListAsync();
            var StyleDetailsServiceModels = _mapper.Map<IList<StyleDetailsServiceModel>>(filteredDbCountries);

            return new PaginationResult<StyleDetailsServiceModel>(pageSize, pageNumber, counter, StyleDetailsServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderAsync(int buyer, string order)
        {

            var filteredDbstyles = await _apparelProDbContext.Styles
                .Where(style => style.BuyerCode == buyer && style.Order == order)
                .AsNoTracking()
                .ToListAsync();

            var StyleDetailsServiceModels = _mapper.Map<IEnumerable<StyleDetailsServiceModel>>(filteredDbstyles);
            return StyleDetailsServiceModels;



            //IQueryable<Style> StylePagination = _apparelProDbContext.Styles
            //  .Where(style => style.BuyerCode == buyer && style.Order == order)
            //  .AsNoTracking();



            //FilterResult fr = new();
            //fr.searchPattern = "{0}.Contains(@0)";
            //fr.FilterColumn = filterColumn;
            //fr.FilterQuery = filterQuery;
            //if (filterColumn != null && filterQuery != null)
            //{
            //    fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Style));
            //    StylePagination = StylePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            //}

            //int counter = 0;
            //counter = await StylePagination.CountAsync();

            //if (sortColumn != null)
            //{
            //    sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
            //    StylePagination = StylePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            //}
            //List<Style>? result = null;

            //var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            //var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            //_distributedCache.TryGetValue<List<Style>>(cacheKey, out result);

            //if (await _distributedCache.GetAsync(cacheKey) == null)
            //{
            //    StylePagination = StylePagination
            //    .Skip(pageSize * pageNumber)
            //        .Take(pageSize);

            //    result = await StylePagination.ToListAsync();

            //    _distributedCache.Set(cacheKey, result, _options);
            //}

            //var filteredDbCountries = result; // await StylePagination.ToListAsync();
            //var StyleDetailsServiceModels = _mapper.Map<IList<StyleDetailsServiceModel>>(filteredDbCountries);

            //return new PaginationResult<StyleDetailsServiceModel>(pageSize, pageNumber, counter, StyleDetailsServiceModels,
            //    sortColumn, sortOrder, filterColumn, filterQuery);
            ////var joined = _apparelProDbContext.Styles
            // .AsNoTracking()
            // .Join(_apparelProDbContext.Buyers, style => style.BuyerCode,
            //     buyer => buyer.BuyerCode, (style, buyer) => new { style, buyer })
            // .AsNoTracking()
            // .Join(_apparelProDbContext.GarmentTypes, (styleAndBuyer) => styleAndBuyer.style.TypeCode,
            //     gt => gt.Id, (style, garmentType) => new { style, garmentType })
            // .AsNoTracking()
            // .Where(final =>
            //     final.style.style.BuyerCode == buyer &&
            //     final.style.style.Order == order                  
            // )
            // .Select(result => new Style
            // {
            //     BuyerCode = result.style.buyer.BuyerCode,
            //     Buyer = result.style.buyer.Name!,
            //     Order = result.style.style.Order,
            //     TypeCode = result.style.style.TypeCode,
            //     Quantity = result.style.style.Quantity,
            //     UnitPrice = result.style.style.UnitPrice,
            //     Unit = result.style.style.Unit,
            //     OrderDate = result.style.style.OrderDate,
            //     ApprovedDate = result.style.style.ApprovedDate,
            //     CustomerReturn = result.style.style.CustomerReturn,
            //     EstimateApprovalDate = result.style.style.EstimateApprovalDate,
            //     EstimateApprovalUserName = result.style.style.EstimateApprovalUserName,
            //     ExportBalance = result.style.style.ExportBalance,
            //     Exported = result.style.style.Exported,
            //     ProductionEndDate = result.style.style.ProductionEndDate,
            //     SupplierReturn = result.style.style.SupplierReturn,
            //     StyleCode = result.style.style.StyleCode,
            //     Username = result.style.style.Username,
            //     Type = result.garmentType.TypeName
            // });

            //IQueryable<Style> stylePagination = (IQueryable<Style>)joined.AsQueryable();
        }
        public async Task<PaginationResult<StyleDetailsServiceModel>> GetStyleDetailsAsync(int pageNumber, 
                int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Style> StylePagination = _apparelProDbContext.Styles
              .AsNoTracking()
              .Join(_apparelProDbContext.Buyers, style => style.BuyerCode,
                  buyer => buyer.BuyerCode, (style, buyer) => new { style, buyer })
              .AsNoTracking()
              .Join(_apparelProDbContext.GarmentTypes, (styleAndBuyer) => styleAndBuyer.style.TypeCode,
                  gt => gt.Id, (style, garmentType) => new { style, garmentType })
              .AsNoTracking()
             .Select(result => new Style
             {
                 BuyerCode = result.style.buyer.BuyerCode,
                 Buyer = result.style.buyer.Name!,
                 Order = result.style.style.Order,
                 TypeCode = result.style.style.TypeCode,
                 Type = result.garmentType.TypeName,
                 Quantity = result.style.style.Quantity,
                 UnitPrice = result.style.style.UnitPrice,
                 Unit = result.style.style.Unit,
                 OrderDate = result.style.style.OrderDate,
                 ApprovedDate = result.style.style.ApprovedDate,
                 CustomerReturn = result.style.style.CustomerReturn,
                 EstimateApprovalDate = result.style.style.EstimateApprovalDate,
                 EstimateApprovalUserName = result.style.style.EstimateApprovalUserName,
                 ExportBalance = result.style.style.ExportBalance,
                 Exported = result.style.style.Exported,
                 ProductionEndDate = result.style.style.ProductionEndDate,
                 SupplierReturn = result.style.style.SupplierReturn,
                 StyleCode = result.style.style.StyleCode,
                 Username = result.style.style.Username                 
             });

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Style));
                StylePagination = StylePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await StylePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                StylePagination = StylePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));               
            }

            List<Style>? result = null;

            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<Style>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                StylePagination = StylePagination
                .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await StylePagination.ToListAsync();

                _distributedCache.Set(cacheKey, result, _options);
            }

            var filteredDbCountries = result; 
            var StyleDetailsServiceModels = _mapper.Map<IList<StyleDetailsServiceModel>>(filteredDbCountries);

            return new PaginationResult<StyleDetailsServiceModel>(pageSize, pageNumber, counter, StyleDetailsServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }       

        public async Task DeleteStyleDetailsAsync(int buyer, string order, int type, string style)
        {
            var dbStyle = _apparelProDbContext.Styles
                .Where(_style=>_style.BuyerCode == buyer && _style.Order == order && _style.TypeCode == type && _style.StyleCode == style)
                .FirstOrDefault();
            _apparelProDbContext.Styles.Remove(dbStyle);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<bool> DoesStyleDetailsExistAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StyleDetailsServiceModel>> FilterStyleDetailsByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }     

        public async Task<StyleDetailsServiceModel> GetStyleDetailsByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByPageNumberAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateStyleDetailsAsync(UpdateStyleDetailsServiceModel updateStyleDetailsServiceModel)
        {            
            var styleDbModel = await _apparelProDbContext.Styles
                .Where(style => 
                    style.BuyerCode == updateStyleDetailsServiceModel.BuyerCode && 
                    style.Order == updateStyleDetailsServiceModel.Order && 
                    style.TypeCode == updateStyleDetailsServiceModel.TypeCode && 
                    style.StyleCode == updateStyleDetailsServiceModel.StyleCode
                )
                .FirstOrDefaultAsync();

            styleDbModel!.TypeCode = updateStyleDetailsServiceModel.TypeCode;
            styleDbModel.StyleCode = updateStyleDetailsServiceModel.StyleCode;
            styleDbModel.Quantity = updateStyleDetailsServiceModel.Quantity;
            styleDbModel.UnitPrice = updateStyleDetailsServiceModel.UnitPrice;
            styleDbModel.Unit = updateStyleDetailsServiceModel.Unit;
            
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<StyleDetailsServiceModel> AddStyleDetailsAsync(CreateStyleDetailsServiceModel createStyleDetailsServiceModel )
        {
            var styleDbModel = _mapper.Map<Style>(createStyleDetailsServiceModel);           
            _apparelProDbContext.Styles.Add(styleDbModel);

            //using var transaction = _apparelProDbContext.Database.BeginTransaction();
            //_apparelProDbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Styles ON");
            //await _apparelProDbContext.SaveChangesAsync();
            //_apparelProDbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Styles OFF");
            //transaction.Commit();
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<StyleDetailsServiceModel>(styleDbModel);
        }
    }
}
