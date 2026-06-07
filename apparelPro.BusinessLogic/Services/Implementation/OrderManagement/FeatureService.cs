using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class FeatureService : IFeatureService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public FeatureService(IMapper mapper, ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
        }

        public async Task<FeatureServiceModel> AddFeatureAsync(CreateFeatureServiceModel createFeatureServiceModel)
        {
            var featureDbModel = _mapper.Map<Feature>(createFeatureServiceModel);
            _apparelProDbContext.Features.Add(featureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<FeatureServiceModel>(featureDbModel);
        }

        public async Task DeleteFeatureAsync(int id)
        {
            var featureDbModel = await _apparelProDbContext.Features
              .Where(feature => feature.Id == id)
              .FirstOrDefaultAsync();
            _apparelProDbContext.Features.Remove(featureDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task<FeatureServiceModel> GetFeatureByIdAsync(int id)
        {
            var featureDbModel = await _apparelProDbContext.Features
             .Where(feature => feature.Id == id)
             .FirstOrDefaultAsync();
            return _mapper.Map<FeatureServiceModel>(featureDbModel);
        }

        public async Task<PaginationResult<FeatureServiceModel>> GetFeaturesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<Feature> featuresPagination = _apparelProDbContext.Features.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(PurchaseOrder));
                featuresPagination = featuresPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }
            int counter = 0;
            counter = await featuresPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                featuresPagination = featuresPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            featuresPagination = featuresPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbPos = await featuresPagination.ToListAsync();
            var featureServiceModels = _mapper.Map<IList<FeatureServiceModel>>(filteredDbPos);

            return new PaginationResult<FeatureServiceModel>(pageSize, pageNumber, counter, featureServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateFeatureAsync(UpdateFeatureServiceModel updateFeatureServiceModel)
        {
            var FeatureDbModel = await _apparelProDbContext.Features
             .Where(Feature => Feature.Id == updateFeatureServiceModel.Id)
             .FirstOrDefaultAsync();

            FeatureDbModel!.Description = updateFeatureServiceModel.Description!;

            _apparelProDbContext.Update(FeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
