using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeDetailsService;

using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using ApparelPro.WebApi.APIModels.OrderManagement;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;
using System.Data;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;


namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class ColorSizeBreakdownDetailsService : IColorSizeBreakdownDetailsService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public ColorSizeBreakdownDetailsService(IMapper mapper, ApparelProDbContext apparelProDbContext,
             ILookupConstants lookupConstants, IDistributedCache distributedCache)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }

        public async Task<ColorSizeBreakdownDetailsServiceModel> AddColorSizeDetailsAsync(CreateColorSizeBreakdownDetailsServiceModel createColorSizeDetailsServiceModel)
        {
            var colorSizeDetailsDbModel = _mapper.Map<ColorSizeDetails>(createColorSizeDetailsServiceModel);
            await _apparelProDbContext.ColorSizeDetails.AddAsync(colorSizeDetailsDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<ColorSizeBreakdownDetailsServiceModel>(colorSizeDetailsDbModel);
        }
        

        public async Task<bool> BulkSaveColorSizeDetailsAsync(int buyerCode, string order, int typeCode, string styleCode,
            List<CreateColorSizeBreakdownDetailsServiceModel> records)
        {
            // f your application has connection resiliency policies enabled (such as automatic retries on SQL Azure),
            // a plain BeginTransaction() will fail because EF Core cannot automatically retry the operations
            // if they fail halfway through.To fix this, you must use context.Database.CreateExecutionStrategy()
            // to wrap your complete block of logic:

            using var context = new ApparelProDbContext();

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                // Wrap the cleanup and insert steps in a single atomic database transaction
                // Setting the isolation level to Serializable or Snapshot
                using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
                try
                {
                    // 1. Locate and purge all previous size-color assignments for this style scope
                    var existingRecords = await _apparelProDbContext.ColorSizeDetails
                        .Where(d => d.BuyerCode == buyerCode &&
                                    d.Order == order &&
                                    d.TypeCode == typeCode &&
                                    d.StyleCode == styleCode)
                        .ToListAsync();

                    if (existingRecords.Any())
                    {
                        _apparelProDbContext.ColorSizeDetails.RemoveRange(existingRecords);
                    }

                    // 2. Map and append the newly structured matrix list payload
                    if (records != null && records.Any())
                    {
                        var dbModels = _mapper.Map<List<ColorSizeDetails>>(records);
                        await _apparelProDbContext.ColorSizeDetails.AddRangeAsync(dbModels);
                    }

                    // 3. Persist and commit changes to your database instance
                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    // Roll back changes cleanly if an error occurs
                    await dbTransaction.RollbackAsync();                    
                    throw;
                    
                }
                

            });
            return false;
        }
    
        public async Task<List<ColorSizeBreakdownDetailsServiceModel>> GetBreakdownByStyleAsync(int buyer, string order, int type, string style)
        {
            // Fetch directly using the composite style foreign key bounds
            var records = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyer &&
                            d.Order == order &&
                            d.TypeCode == type &&
                            d.StyleCode == style)
                .ToListAsync();

            var mappedResult = _mapper.Map<List<ColorSizeBreakdownDetailsServiceModel>>(records);
            return mappedResult;
        }

        public async Task<PaginationResult<ColorSizeBreakdownDetailsServiceModel>> GetColorSizeDetailsAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ColorSizeDetails> ColorSizeDetailsPagination = _apparelProDbContext.ColorSizeDetails.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(ColorSizeDetails));
                ColorSizeDetailsPagination = ColorSizeDetailsPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await ColorSizeDetailsPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                ColorSizeDetailsPagination = ColorSizeDetailsPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            List<ColorSizeDetails>? result = null;

           
            ColorSizeDetailsPagination = ColorSizeDetailsPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            result = await ColorSizeDetailsPagination.ToListAsync();

            var filteredDbColorSizeDetails = result;
            var ColorSizeDetailserviceModels = _mapper.Map<IList<ColorSizeBreakdownDetailsServiceModel>>(filteredDbColorSizeDetails);

            return new PaginationResult<ColorSizeBreakdownDetailsServiceModel>(pageSize, pageNumber, counter, ColorSizeDetailserviceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<StyleDimensionsLookupServiceModel> GetStyleDimensionsAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();

            // Query distinct colors mapped to this active style matrix workspace context
            var activeColors = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode &&
                            d.Order == order &&
                            d.TypeCode == typeCode &&
                            d.StyleCode == styleCode &&
                            d.Color != null && d.Color != "")
                .Select(d => d.Color)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Query distinct sizes mapped to this active style matrix workspace context
            var activeSizes = await _apparelProDbContext.ColorSizeDetails
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode &&
                            d.Order == order &&
                            d.TypeCode == typeCode &&
                            d.StyleCode == styleCode &&
                            d.Size != null && d.Size != "")
                .Select(d => d.Size)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return new StyleDimensionsLookupServiceModel
            {
                Colors = activeColors,
                Sizes = activeSizes
            };
        }

        //public async Task<List<ColorSizeDetails>> GetSavedColorSizeMatrixAsync1(int buyerCode, string order, int typeCode, string styleCode)
        //{
        //    return await _apparelProDbContext.ColorSizeDetails
        //        .AsNoTracking()
        //        .Where(d => d.BuyerCode == buyerCode &&
        //                    d.Order == order.Trim() &&
        //                    d.TypeCode == typeCode &&
        //                    d.StyleCode == styleCode.Trim())
        //        .OrderBy(d => d.Color)
        //        .ThenBy(d => d.Size)
        //        .ToListAsync();
        //}

        public async Task<List<ColorSizeDetails>> GetSavedColorSizeMatrixAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            return await _apparelProDbContext.ColorSizeDetails
                 .AsNoTracking()
                 .Where(d => d.BuyerCode == buyerCode &&
                             d.Order == order.Trim() &&
                             d.TypeCode == typeCode &&
                             d.StyleCode == styleCode.Trim())
                 .OrderBy(d => d.Color)
                 .ThenBy(d => d.Size)
                 .ToListAsync();
        }

    }
}
