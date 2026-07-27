using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using ApparelPro.Data;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class ItemFeatureService : IItemFeatureService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        public ItemFeatureService(IMapper mapper, ApparelProDbContext apparelProDbContext, ILookupConstants lookupConstants)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _lookupConstants = lookupConstants;
        }

        public async Task<ItemFeatureServiceModel> AddItemFeatureAsync(CreateItemFeatureServiceModel createItemFeatureServiceModel)
        {
            var itemFeatureDbModel = _mapper.Map<Feature>(createItemFeatureServiceModel);
            _apparelProDbContext.Features.Add(itemFeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<ItemFeatureServiceModel>(itemFeatureDbModel);
        }

        public async Task DeleteFeatureAsync(int id)
        {
            var featureDbModel = await _apparelProDbContext.Features
              .Where(feature => feature.Id == id)
              .FirstOrDefaultAsync();
            _apparelProDbContext.Features.Remove(featureDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public Task DeleteItemFeatureAsync(string featureCode)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemFeatureServiceModel> GetItemFeatureByFeatureCodeAsync(string featureCode)
        {
            var itemFeatureDbModel = await _apparelProDbContext.ItemFeatures
             .Where(feature => feature.FeatureCode == featureCode)
             .FirstOrDefaultAsync();
            return _mapper.Map<ItemFeatureServiceModel>(itemFeatureDbModel);
        }

        public async Task<PaginationResult<ItemFeatureServiceModel>> GetItemFeaturesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<ItemFeature> featuresPagination = _apparelProDbContext.ItemFeatures.AsNoTracking();

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
            var featureServiceModels = _mapper.Map<IList<ItemFeatureServiceModel>>(filteredDbPos);

            return new PaginationResult<ItemFeatureServiceModel>(pageSize, pageNumber, counter, featureServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateItemFeatureAsync(UpdateItemFeatureServiceModel updateItemFeatureServiceModel)
        {
            var ItemFeatureDbModel = await _apparelProDbContext.ItemFeatures
             .Where(Feature => Feature.FeatureCode == updateItemFeatureServiceModel.FeatureCode)
             .FirstOrDefaultAsync();

            ItemFeatureDbModel!.Description = updateItemFeatureServiceModel.Description!;

            _apparelProDbContext.Update(ItemFeatureDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
