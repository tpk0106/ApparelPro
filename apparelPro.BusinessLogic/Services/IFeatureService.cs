using apparelPro.BusinessLogic.Services.Models.Reference.IFeatureService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IFeatureService
    {
        Task<PaginationResult<FeatureServiceModel>> GetFeaturesAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, 
            string? filterColumn, string? filterQuery);
        Task<FeatureServiceModel> GetFeatureByIdAsync(int id);
        Task<FeatureServiceModel> AddFeatureAsync(CreateFeatureServiceModel createFeatureServiceModel);
        Task UpdateFeatureAsync(UpdateFeatureServiceModel updateFeatureServiceModel);
        Task DeleteFeatureAsync(int id);
    }
}
