using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IUnitService
    {
        Task<PaginationResult<UnitServiceModel>> GetUnitsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

       // Task<IEnumerable<UnitServiceModel>> GetUnitsAsync();
        Task<UnitServiceModel> GetUnitByCodeAsync(string code);
        Task<UnitServiceModel> AddUnitAsync(CreateUnitServiceModel createUnitServiceModel);
        Task UpdateUnitAsync(UpdateUnitServiceModel updateUnitServiceModel);
        Task DeleteUnitAsync(string code);
        Task<bool> DoesUnitExistAsync(string code);
    }
}
