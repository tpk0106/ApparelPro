using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IItemFeatureService
    {
        Task<PaginationResult<ItemFeatureServiceModel>> GetItemFeaturesAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, 
            string? filterColumn, string? filterQuery);
        Task<ItemFeatureServiceModel> GetItemFeatureByFeatureCodeAsync(string featureCode);
        Task<ItemFeatureServiceModel> AddItemFeatureAsync(CreateItemFeatureServiceModel  createItemFeatureServiceModel);
        Task UpdateItemFeatureAsync(UpdateItemFeatureServiceModel  updateItemFeatureServiceModel);
        Task DeleteItemFeatureAsync(string featureCode);
    }
}
