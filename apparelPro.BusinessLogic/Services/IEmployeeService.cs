using apparelPro.BusinessLogic.Services.Models.Production.IEmployeeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IEmployeeService
    {
        Task<PaginationResult<EmployeeServiceModel>> GetEmployeesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<EmployeeServiceModel?> GetEmployeeByEmployeeCodeAsync(string employeeCode);
        Task<EmployeeServiceModel> AddEmployeeAsync(CreateEmployeeServiceModel createServiceModel);
        Task UpdateEmployeeAsync(UpdateEmployeeServiceModel updateServiceModel);
        Task DeleteEmployeeAsync(string employeeCode);
    }
}
