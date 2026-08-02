using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.interfaces.ISharedService;
using apparelPro.BusinessLogic.Services.Implementation.Registration;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class StyleDetailsService : IStyleDetailsService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
        private readonly ISharedService _sharedService;
        private readonly ISystemParameterService _systemParameterService;

        private const string AllowOrderQuantityOverrideParameterKey = "AllowOrderQuantityOverride";

        public StyleDetailsService(IMapper mapper,  ApparelProDbContext apparelProDbContext,
             ILookupConstants lookupConstants, IDistributedCache distributedCache,
             ISharedService sharedService, ISystemParameterService systemParameterService)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
            _sharedService = sharedService;
            _systemParameterService = systemParameterService;
        }

        /// <summary>
        /// Sums every style's Quantity for this buyer+order, each converted into the order's own
        /// unit (mirrors legacy OD_STY1.PRG's running "TOTAL" - convert(unit, xunit, qty)).
        /// Optionally excludes one existing style row by its Type+Style key, so validating an
        /// UPDATE doesn't double-count that row's old quantity alongside its incoming new one.
        /// </summary>
        private async Task<(decimal TotalQuantity, PurchaseOrder PurchaseOrder)> ComputeConvertedStyleTotalAsync(
            int buyerCode, string order, int? excludeTypeCode = null, string? excludeStyleCode = null)
        {
            var purchaseOrder = await _apparelProDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.BuyerCode == buyerCode && po.Order == order);

            if (purchaseOrder == null)
            {
                throw new InvalidOperationException($"No Purchase Order found for Buyer {buyerCode} / Order '{order}'.");
            }

            var existingStyles = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order)
                .ToListAsync();

            decimal total = 0;
            foreach (var style in existingStyles)
            {
                if (excludeTypeCode.HasValue && excludeStyleCode != null &&
                    style.TypeCode == excludeTypeCode.Value && style.StyleCode == excludeStyleCode)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(style.Unit) || !style.Quantity.HasValue) continue;

                total += await _sharedService.ConvertUnitAsync(style.Unit, purchaseOrder.UnitCode, style.Quantity.Value);
            }

            return (total, purchaseOrder);
        }

        /// <summary>
        /// Enforces that a style's quantity (converted into the order's unit) does not push the
        /// order's running style total past its Total Quantity, unless the 'AllowOrderQuantityOverride'
        /// system parameter is turned on. This also implicitly validates the style's unit is
        /// convertible to the order's unit at all: ConvertUnitAsync throws InvalidOperationException
        /// itself when the Unit Conversion table has no rule relating the two units - mirroring
        /// legacy OD_STY1.PRG's chk_unit()/convert() pair (od_conv, keyed f_unit+t_unit).
        ///
        /// Before any of that: if a Supplier Purchase Order has already been raised against this
        /// Buyer+Order (a PODetails row exists), style quantities are frozen entirely - this lock
        /// is absolute and is checked regardless of the AllowOrderQuantityOverride setting.
        ///
        /// When an override is allowed and actually used to let a save through, the current
        /// user's email and a UTC timestamp are stamped onto the order's (tracked) PurchaseOrder
        /// entity, so the audit trail records who authorized exceeding the confirmed quantity and
        /// when. The caller's own SaveChangesAsync() persists this alongside the style row.
        /// </summary>
        private async Task ValidateStyleQuantityAsync(
            int buyerCode, string order, string unit, decimal quantity, string? currentUserEmail,
            int? excludeTypeCode = null, string? excludeStyleCode = null)
        {
            var hasSupplierPurchaseOrder = await _apparelProDbContext.PODetails
                .AsNoTracking()
                .AnyAsync(poDetail => poDetail.Buyer == buyerCode && poDetail.Order == order);

            if (hasSupplierPurchaseOrder)
            {
                throw new InvalidOperationException(
                    $"Style quantities for Buyer {buyerCode} / Order '{order}' can no longer be changed - " +
                    "a Supplier Purchase Order has already been raised against this order.");
            }

            var (existingTotal, purchaseOrder) = await ComputeConvertedStyleTotalAsync(buyerCode, order, excludeTypeCode, excludeStyleCode);

            var convertedQuantity = await _sharedService.ConvertUnitAsync(unit, purchaseOrder.UnitCode, quantity);
            var newTotal = existingTotal + convertedQuantity;

            if (newTotal > purchaseOrder.TotalQuantity)
            {
                var allowOverride = await _systemParameterService.GetBoolValueAsync(AllowOrderQuantityOverrideParameterKey);
                if (!allowOverride)
                {
                    throw new InvalidOperationException(
                        $"Style quantities would total {newTotal:0.##} {purchaseOrder.UnitCode}, exceeding the order's Total " +
                        $"Quantity of {purchaseOrder.TotalQuantity:0.##} {purchaseOrder.UnitCode}. Turn on 'Allow Order Quantity " +
                        "Override' in System Parameters if this order intentionally needs to exceed its confirmed quantity.");
                }

                var trackedPurchaseOrder = await _apparelProDbContext.PurchaseOrders
                    .FirstOrDefaultAsync(po => po.BuyerCode == buyerCode && po.Order == order);

                if (trackedPurchaseOrder != null)
                {
                    trackedPurchaseOrder.QuantityOverriddenBy = currentUserEmail;
                    trackedPurchaseOrder.QuantityOverriddenAt = DateTime.UtcNow;
                }
            }
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

            var filteredDbStyles = result; 
            var StyleDetailsServiceModels = _mapper.Map<IList<StyleDetailsServiceModel>>(filteredDbStyles);

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

        public async Task UpdateStyleDetailsAsync(UpdateStyleDetailsServiceModel updateStyleDetailsServiceModel, string? currentUserEmail = null)
        {
            var styleDbModel = await _apparelProDbContext.Styles
                .Where(style =>
                    style.BuyerCode == updateStyleDetailsServiceModel.BuyerCode &&
                    style.Order == updateStyleDetailsServiceModel.Order &&
                    style.TypeCode == updateStyleDetailsServiceModel.TypeCode &&
                    style.StyleCode == updateStyleDetailsServiceModel.StyleCode
                )
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(updateStyleDetailsServiceModel.Unit) && updateStyleDetailsServiceModel.Quantity.HasValue)
            {
                await ValidateStyleQuantityAsync(
                    updateStyleDetailsServiceModel.BuyerCode,
                    updateStyleDetailsServiceModel.Order,
                    updateStyleDetailsServiceModel.Unit,
                    updateStyleDetailsServiceModel.Quantity.Value,
                    currentUserEmail,
                    excludeTypeCode: updateStyleDetailsServiceModel.TypeCode,
                    excludeStyleCode: updateStyleDetailsServiceModel.StyleCode);
            }

            styleDbModel!.TypeCode = updateStyleDetailsServiceModel.TypeCode;
            styleDbModel.StyleCode = updateStyleDetailsServiceModel.StyleCode;
            styleDbModel.Quantity = updateStyleDetailsServiceModel.Quantity;
            styleDbModel.UnitPrice = updateStyleDetailsServiceModel.UnitPrice;
            styleDbModel.Unit = updateStyleDetailsServiceModel.Unit;

            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<StyleDetailsServiceModel> AddStyleDetailsAsync(CreateStyleDetailsServiceModel createStyleDetailsServiceModel, string? currentUserEmail = null)
        {
            if (!string.IsNullOrWhiteSpace(createStyleDetailsServiceModel.Unit) && createStyleDetailsServiceModel.Quantity.HasValue)
            {
                await ValidateStyleQuantityAsync(
                    createStyleDetailsServiceModel.BuyerCode,
                    createStyleDetailsServiceModel.Order,
                    createStyleDetailsServiceModel.Unit,
                    createStyleDetailsServiceModel.Quantity.Value,
                    currentUserEmail);
            }

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

        public async Task<StyleTotalsServiceModel> GetStyleTotalsAsync(int buyerCode, string order)
        {
            var (totalQuantity, purchaseOrder) = await ComputeConvertedStyleTotalAsync(buyerCode, order);

            return new StyleTotalsServiceModel
            {
                TotalQuantity = totalQuantity,
                MainUnit = purchaseOrder.UnitCode,
                OrderTotalQuantity = purchaseOrder.TotalQuantity,
                ExceedsOrderQuantity = totalQuantity > purchaseOrder.TotalQuantity
            };
        }

        public async Task<IEnumerable<StyleDetailsServiceModel>> GetStyleDetailsByBuyerOrderTypeAsync(int buyerCode, string order, int typeCode)
        {
            var allStyleCodes = await _apparelProDbContext.Styles
                .AsNoTracking()                
                .Where(style => style.BuyerCode == buyerCode && style.Order == order && style.TypeCode == typeCode)               
                .ToListAsync();

            var styleDetailsServiceModels = _mapper.Map<IEnumerable<StyleDetailsServiceModel>>(allStyleCodes);
            return  styleDetailsServiceModels;
        }

        public async Task<StyleApprovalDetailsServiceModel> GetEstimateApprovalUserNameAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var styleApprovalDetailsServiceModel = await _apparelProDbContext.Styles
              .AsNoTracking()
              .Where(style => style.BuyerCode == buyerCode &&
                  style.Order == order && style.TypeCode == typeCode &&
                  style.StyleCode == styleCode)
              .Select(style => new StyleApprovalDetailsServiceModel
              {
                  EstimateApprovalDate = style.ApprovedDate,
                  EstimateApprovalUserName = style.EstimateApprovalUserName
              })
              .FirstOrDefaultAsync();

            return styleApprovalDetailsServiceModel;
        }
    }
}