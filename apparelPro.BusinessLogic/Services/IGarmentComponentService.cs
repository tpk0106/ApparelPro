using apparelPro.BusinessLogic.Services.Models.Production.IGarmentComponentService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IGarmentComponentService
    {
        Task<PaginationResult<GarmentComponentServiceModel>> GetGarmentComponentsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<GarmentComponentServiceModel?> GetGarmentComponentByComponentCodeAsync(string componentCode);
        Task<GarmentComponentServiceModel> AddGarmentComponentAsync(CreateGarmentComponentServiceModel createServiceModel);
        Task UpdateGarmentComponentAsync(UpdateGarmentComponentServiceModel updateServiceModel);
        Task DeleteGarmentComponentAsync(string componentCode);
    }
}
