using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsProcedureCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICustomsProcedureCodeService
    {
        Task<PaginationResult<CustomsProcedureCodeServiceModel>> GetCustomsProcedureCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<CustomsProcedureCodeServiceModel> GetCustomsProcedureCodeByCodeAsync(string code);
        Task<CustomsProcedureCodeServiceModel> AddCustomsProcedureCodeAsync(CreateCustomsProcedureCodeServiceModel createCustomsProcedureCodeServiceModel);
        Task UpdateCustomsProcedureCodeAsync(UpdateCustomsProcedureCodeServiceModel updateCustomsProcedureCodeServiceModel);
        Task DeleteCustomsProcedureCodeAsync(string code);
    }
}
