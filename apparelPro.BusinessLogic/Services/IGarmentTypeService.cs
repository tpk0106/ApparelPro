using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IGarmentTypeService
    {
        Task<PaginationResult<GarmentTypeServiceModel>> GetGarmentTypesAsync(int pageNumber, int pageSize, 
            string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);        
        Task<IEnumerable<GarmentTypeServiceModel>> GetGarmentTypesByPageNumberAsync(int pageNumber, int pageSize);
        Task<IEnumerable<GarmentTypeServiceModel>> GetAllGarmentTypeAsync();
        Task<GarmentTypeServiceModel> GetGarmentTypeByIdAsync(int id);
        Task<GarmentTypeServiceModel> CreateGarmentTypeAsync(CreateGarmentTypeServiceModel createGarmentTypeServiceModel);
        Task UpdateGarmentTypeAsync(UpdateGarmentTypeServiceModel updateGarmentTypeServiceModel);
        Task DeleteGarmentTypeAsync(int id);
    }
}
