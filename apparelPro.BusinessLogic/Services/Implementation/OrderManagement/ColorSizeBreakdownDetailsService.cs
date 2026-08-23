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

        // SUPPLIER PO LOCK GUARD: once a Supplier Purchase Order has been raised against this
        // exact Buyer/Order/Type/Style, the colour/size allocation that fed its material
        // consumption calculation is frozen - absolute, no override, matching the same real-world
        // reasoning as Style.Quantity's own Supplier PO lock (StyleDetailsService
        // .ValidateStyleQuantityAsync). Scoped to the exact style (not the whole order) since a
        // Supplier PO is raised per style - locking every style in the order would be too broad.
        private async Task EnsureNotLockedBySupplierPoAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            var hasSupplierPurchaseOrder = await _apparelProDbContext.SupplierPurchaseOrderDetails
                .AsNoTracking()
                .AnyAsync(d => d.Buyer == buyerCode && d.Order == order && d.Type == typeCode && d.Style == styleCode);

            if (hasSupplierPurchaseOrder)
                throw new InvalidOperationException(
                    $"Colour/Size Breakdown for Style '{styleCode}' can no longer be changed - " +
                    "a Supplier Purchase Order has already been raised against this style.");
        }

        public async Task<ColorSizeBreakdownDetailsServiceModel> AddColorSizeDetailsAsync(CreateColorSizeBreakdownDetailsServiceModel createColorSizeDetailsServiceModel)
        {
            await EnsureNotLockedBySupplierPoAsync(
                createColorSizeDetailsServiceModel.BuyerCode,
                createColorSizeDetailsServiceModel.Order,
                createColorSizeDetailsServiceModel.TypeCode,
                createColorSizeDetailsServiceModel.StyleCode);

            var colorSizeDetailsDbModel = _mapper.Map<ColorSizeDetails>(createColorSizeDetailsServiceModel);
            await _apparelProDbContext.ColorSizeDetails.AddAsync(colorSizeDetailsDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<ColorSizeBreakdownDetailsServiceModel>(colorSizeDetailsDbModel);
        }


        public async Task<bool> BulkSaveColorSizeDetailsAsync(int buyerCode, string order, int typeCode, string styleCode,
            List<CreateColorSizeBreakdownDetailsServiceModel> records)
        {
            if (records == null || !records.Any())
                throw new InvalidOperationException("At least one size matrix row is required.");

            await EnsureNotLockedBySupplierPoAsync(buyerCode, order, typeCode, styleCode);

            var style = await _apparelProDbContext.Styles
                .AsNoTracking()
                .Where(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode)
                .FirstOrDefaultAsync();

            if (style == null)
                throw new InvalidOperationException("Style not found for the given Buyer/Order/Type/Style.");

            // The Colour stage (ColorQuantityRatios) is the authoritative
            // per-colour target this size matrix must reconcile against - it
            // must already be saved, and every colour here must exactly match
            // the colours saved there (no orphaned colour on either side).
            var colourAllocations = await _apparelProDbContext.ColorQuantityRatios
                .AsNoTracking()
                .Where(d => d.BuyerCode == buyerCode && d.Order == order && d.TypeCode == typeCode && d.StyleCode == styleCode)
                .ToDictionaryAsync(d => d.Color, d => d.Quantity);

            var groupsByColor = records.GroupBy(r => r.Color).ToList();

            var postedColors = groupsByColor.Select(g => g.Key).ToHashSet();
            var missingFromColourStage = postedColors.Except(colourAllocations.Keys).ToList();
            if (missingFromColourStage.Any())
                throw new InvalidOperationException(
                    $"Colour(s) {string.Join(", ", missingFromColourStage)} have no saved Colour-stage allocation - save the Colour Target Allocation step first.");

            var missingFromSizeMatrix = colourAllocations.Keys.Except(postedColors).ToList();
            if (missingFromSizeMatrix.Any())
                throw new InvalidOperationException(
                    $"Colour(s) {string.Join(", ", missingFromSizeMatrix)} are missing from the size matrix - every allocated colour needs a full size breakdown.");

            var isRatioMode = string.Equals(style.SizeRatio?.Trim(), "R", StringComparison.OrdinalIgnoreCase);

            foreach (var group in groupsByColor)
            {
                var target = (int)Math.Round(colourAllocations[group.Key], MidpointRounding.AwayFromZero);
                var rows = group.ToList();

                if (isRatioMode)
                {
                    var weights = rows.Select(r => r.Ratio).ToList();
                    var allocated = RatioAllocator.Allocate(target, weights);
                    for (var i = 0; i < rows.Count; i++)
                    {
                        rows[i].Quantity = allocated[i];
                    }
                }
                else
                {
                    var postedTotal = rows.Sum(r => r.Quantity);
                    if (postedTotal != target)
                        throw new InvalidOperationException(
                            $"Size matrix for colour '{group.Key}' does not reconcile: entered total is {postedTotal:N2} Pcs but its Colour-stage allocation is {target:N2} Pcs.");
                    foreach (var r in rows)
                    {
                        r.Ratio = 0;
                    }
                }
            }

            var strategy = _apparelProDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await _apparelProDbContext.Database.BeginTransactionAsync(isolationLevel: IsolationLevel.Snapshot);
                try
                {
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

                    var dbModels = _mapper.Map<List<ColorSizeDetails>>(records);
                    await _apparelProDbContext.ColorSizeDetails.AddRangeAsync(dbModels);

                    await _apparelProDbContext.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });

            return true;
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
