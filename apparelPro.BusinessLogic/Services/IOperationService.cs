using apparelPro.BusinessLogic.Services.Models.Production.IOperationService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IOperationService
    {
        Task<PaginationResult<OperationServiceModel>> GetOperationsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<OperationServiceModel?> GetOperationByOperationCodeAsync(string operationCode);
        Task<OperationServiceModel> AddOperationAsync(CreateOperationServiceModel createServiceModel);
        Task UpdateOperationAsync(UpdateOperationServiceModel updateServiceModel);
        Task DeleteOperationAsync(string operationCode);
    }
}
