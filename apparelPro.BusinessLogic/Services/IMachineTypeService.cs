using apparelPro.BusinessLogic.Services.Models.Production.IMachineTypeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IMachineTypeService
    {
        Task<PaginationResult<MachineTypeServiceModel>> GetMachineTypesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<MachineTypeServiceModel?> GetMachineTypeByCodeAsync(string code);
        Task<MachineTypeServiceModel> AddMachineTypeAsync(CreateMachineTypeServiceModel createServiceModel);
        Task UpdateMachineTypeAsync(UpdateMachineTypeServiceModel updateServiceModel);
        Task DeleteMachineTypeAsync(string code);
    }
}
