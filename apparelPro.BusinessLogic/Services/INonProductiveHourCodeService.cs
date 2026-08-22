using apparelPro.BusinessLogic.Services.Models.Production.INonProductiveHourCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface INonProductiveHourCodeService
    {
        Task<PaginationResult<NonProductiveHourCodeServiceModel>> GetNonProductiveHourCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<NonProductiveHourCodeServiceModel?> GetNonProductiveHourCodeByCodeAsync(string code);
        Task<NonProductiveHourCodeServiceModel> AddNonProductiveHourCodeAsync(CreateNonProductiveHourCodeServiceModel createServiceModel);
        Task UpdateNonProductiveHourCodeAsync(UpdateNonProductiveHourCodeServiceModel updateServiceModel);
        Task DeleteNonProductiveHourCodeAsync(string code);
    }
}
