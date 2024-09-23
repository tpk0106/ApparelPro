using apparelPro.BusinessLogic.Extensions;
using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class UnitService:IUnitService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public UnitService(IMapper mapper, ApparelProDbContext apparelProDbContext,
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
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
            _distributedCache = distributedCache;
        }

        public async Task<UnitServiceModel> AddUnitAsync(CreateUnitServiceModel createUnitServiceModel)
        {
            var unitDbModel = _mapper.Map<Unit>(createUnitServiceModel);
            _apparelProDbContext.Units.Add(unitDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<UnitServiceModel>(unitDbModel);
        }      

        public async Task<PaginationResult<UnitServiceModel>> GetUnitsAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
             IQueryable<Unit> unitPagination = _apparelProDbContext.Units.AsNoTracking();
            
            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Unit));
                unitPagination = unitPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await unitPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                unitPagination = unitPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));               
            }

            List<Unit>? result = null;

            var cacheKey = $"{pageNumber}-{pageSize}-{sortColumn}-{sortOrder}-{filterColumn}-{filterQuery}";
            var _options = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30) };

            _distributedCache.TryGetValue<List<Unit>>(cacheKey, out result);

            if (await _distributedCache.GetAsync(cacheKey) == null)
            {
                unitPagination = unitPagination
                    .Skip(pageSize * pageNumber)
                    .Take(pageSize);

                result = await unitPagination.ToListAsync();

                _distributedCache.Set(cacheKey, result, _options);
            }

            unitPagination = unitPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCountries = result;
            var UnitServiceModels = _mapper.Map<IList<UnitServiceModel>>(filteredDbCountries);

            return new PaginationResult<UnitServiceModel>(pageSize, pageNumber, counter, UnitServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<UnitServiceModel> GetUnitByCodeAsync(string code)
        {
            var unitDbModel = await _apparelProDbContext.Units.Where(unit => unit.Code == code).FirstOrDefaultAsync();
            var unitServiceModel = _mapper.Map<UnitServiceModel>(unitDbModel);
            return unitServiceModel;
        }

        public async Task<IEnumerable<UnitServiceModel>> GetUnitsAsync()
        {
            var unitDbModels = await _apparelProDbContext.Units.ToListAsync();
            var unitServiceModels = _mapper.Map<IEnumerable<UnitServiceModel>>(unitDbModels);
            return unitServiceModels;
        }

        public async Task UpdateUnitAsync(UpdateUnitServiceModel updateUnitServiceModel)
        {
            var unitDbModel = await _apparelProDbContext.Units
               .Where(unit => unit.Code == updateUnitServiceModel.Code)
               .FirstOrDefaultAsync();

            unitDbModel!.Code = updateUnitServiceModel.Code;
            unitDbModel.Description = updateUnitServiceModel.Description;
            
            await _apparelProDbContext.SaveChangesAsync();
        }      

        public async Task<bool> DoesUnitExistAsync(string code)
        {
            IQueryable<Unit> units = _apparelProDbContext.Units;
            var unitDbModel = await units.Where(unit => unit.Code == code)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return unitDbModel != null;
        }
        public Task DeleteUnitAsync(string code)
        {
            throw new NotImplementedException();
        }
    }
}
