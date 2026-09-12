using apparelPro.BusinessLogic.Services.Models.ImportExport.IClearanceOfficeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IClearanceOfficeService
    {
        Task<PaginationResult<ClearanceOfficeServiceModel>> GetClearanceOfficesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<ClearanceOfficeServiceModel> GetClearanceOfficeByCodeAsync(string code);
        Task<ClearanceOfficeServiceModel> AddClearanceOfficeAsync(CreateClearanceOfficeServiceModel createClearanceOfficeServiceModel);
        Task UpdateClearanceOfficeAsync(UpdateClearanceOfficeServiceModel updateClearanceOfficeServiceModel);
        Task DeleteClearanceOfficeAsync(string code);
    }
}
