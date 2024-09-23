using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using ApparelPro.Data.Models.References;
using ApparelPro.Data;
using ApparelPro.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class GarmentTypeService : IGarmentTypeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public GarmentTypeService(IMapper mapper, ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;            
        }
     
        public async Task<GarmentTypeServiceModel> GetGarmentTypeByIdAsync(int id)
        {
            IQueryable<GarmentType> garmentType = _apparelProDbContext.GarmentTypes;
            var garmentTypeDbModel = await garmentType.Where(garmentType => garmentType.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
          
            var garmentTypeServiceModel = _mapper.Map<GarmentTypeServiceModel>(garmentTypeDbModel);
            return garmentTypeServiceModel;
        }

        public async Task<PaginationResult<GarmentTypeServiceModel>> GetGarmentTypesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
             IQueryable<GarmentType> GarmentTypesPagination = _apparelProDbContext.GarmentTypes.AsNoTracking();
          
            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(GarmentType));
                GarmentTypesPagination = GarmentTypesPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await GarmentTypesPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                GarmentTypesPagination = GarmentTypesPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
              
            }
            GarmentTypesPagination = GarmentTypesPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbCountries = await GarmentTypesPagination.ToListAsync();
            var garmentTypeServiceModels = _mapper.Map<IList<GarmentTypeServiceModel>>(filteredDbCountries);

            return new PaginationResult<GarmentTypeServiceModel>(pageSize, pageNumber, counter, garmentTypeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }     

        public async Task UpdateGarmentTypeAsync(UpdateGarmentTypeServiceModel updateGarmentTypeServiceModel)
        {
            var garmentTypeDbModel = await _apparelProDbContext.GarmentTypes
              .Where(garmentType => garmentType.Id == updateGarmentTypeServiceModel.Id)
              .FirstOrDefaultAsync();

            garmentTypeDbModel!.TypeName = updateGarmentTypeServiceModel.TypeName;           
            await _apparelProDbContext.SaveChangesAsync();
        }

        public Task<IEnumerable<GarmentTypeServiceModel>> GetGarmentTypesByPageNumberAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<GarmentTypeServiceModel> AddGarmentTypeAsync(CreateGarmentTypeServiceModel createGarmentTypeServiceModel)
        {
            throw new NotImplementedException();
        }

        public Task DeleteGarmentTypeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GarmentTypeServiceModel>> FilterGarmentTypeByCodeAsync(string filter, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
