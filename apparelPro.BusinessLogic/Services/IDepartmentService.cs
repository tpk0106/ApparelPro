using apparelPro.BusinessLogic.Services.Models.Reference.IDepartmentService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDepartmentService
    {
        Task<PaginationResult<DepartmentServiceModel>> GetDepartmentsAsync(int pageNumber, int pageSize, string? 
            sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);

        Task<IEnumerable<DepartmentServiceModel>> GetDepartmentsLookup();
    }
}
