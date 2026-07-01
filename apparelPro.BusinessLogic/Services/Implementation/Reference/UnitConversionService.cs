using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitConversionService;

using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class UnitConversionService : IUnitConversionService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly IDistributedCache _distributedCache;
        public UnitConversionService(IMapper mapper, ApparelProDbContext apparelProDbContext, IDistributedCache distributedCache)
        {
            _apparelProDbContext = apparelProDbContext;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }

        public Task<UnitConversionServiceModel> AddUnitAsync(CreateUnitConversionServiceModel createUnitConversionServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUnitConversionAsync(string fromUnit, string toUnit)
        {
            throw new NotImplementedException();
        }

        public async Task<UnitConversionServiceModel> GetUnitConversionByFromUnitAndToUnitAsync(string fromUnit, string toUnit)
        {
            var unitConversionDbModel = await _apparelProDbContext
                .UnitConversion.Where(uc => uc.FromUnit == fromUnit && uc.ToUnit == toUnit)
                .FirstOrDefaultAsync();
            var unitConversionServiceModel = _mapper.Map<UnitConversionServiceModel>(unitConversionDbModel);
            return unitConversionServiceModel;                
        }

        public async Task<PaginationResult<UnitConversionServiceModel>> GetUnitConversionsAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<UnitConversion> UnitConversionPagination = _apparelProDbContext.UnitConversion.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(UnitConversion));
                UnitConversionPagination = UnitConversionPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await UnitConversionPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                UnitConversionPagination = UnitConversionPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));               
            }

            List<UnitConversion>? result = null;

            // var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            // Add a unique "countries:" namespace prefix to the string
            var cacheKey = $"unit-conversions:{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";

            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<UnitConversion>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                UnitConversionPagination = UnitConversionPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await UnitConversionPagination.ToListAsync();

                _distributedCache.Set(cacheKey, result, _options);
            }

            var filteredDbCountries = result;
            var UnitConversionServiceModels = _mapper.Map<IList<UnitConversionServiceModel>>(filteredDbCountries);

            return new PaginationResult<UnitConversionServiceModel>(pageSize, pageNumber, counter, UnitConversionServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public Task UpdateUnitAsync(UpdateUnitConversionServiceModel updateUnitConversionServiceModel)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> ConvertUnitAsync(string fromUnit, string toUnit, decimal quantity)
        {
            fromUnit = fromUnit.Trim().ToUpper();
            toUnit = toUnit.Trim().ToUpper();

            // Base Case Guard: No calculation needed if the unit codes are identical
            if (fromUnit == toUnit) return quantity;

            // 1. Look for a Forward Conversion rule (From Left to Right, e.g., GRS -> PCS)
            var forwardRule = await _apparelProDbContext.UnitConversion
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FromUnit == fromUnit && u.ToUnit == toUnit);

            if (forwardRule != null && forwardRule.Measure.HasValue)
            {
                // Multiply when moving forward from Left to Right
                return quantity * forwardRule.Measure.Value;
            }

            // 2. Look for a Reverse Conversion rule (From Right to Left, e.g., PCS -> GRS)
            var reverseRule = await _apparelProDbContext.UnitConversion
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FromUnit == toUnit && u.ToUnit == fromUnit);

            if (reverseRule != null && reverseRule.Measure.HasValue)
            {
                // Safe Division Guard to prevent application crashes
                if (reverseRule.Measure.Value == 0) return 0;

                // Divide when moving backward from Right to Left
                return quantity / reverseRule.Measure.Value;
            }

            // 3. System Error Trap: Block execution if no relational mapping rule exists in the database
            throw new InvalidOperationException($"Unit Conversion Map Error: No conversion rule exists between unit code '{fromUnit}' and unit code '{toUnit}' inside the lookup tables.");
        }
    }
}
