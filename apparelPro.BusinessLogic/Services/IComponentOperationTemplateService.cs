using apparelPro.BusinessLogic.Services.Models.Production.IComponentOperationTemplateService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IComponentOperationTemplateService
    {
        Task<PaginationResult<ComponentOperationTemplateServiceModel>> GetComponentOperationTemplatesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<List<ComponentOperationTemplateServiceModel>> GetTemplatesByComponentAsync(string componentCode);
        Task<ComponentOperationTemplateServiceModel> AddComponentOperationTemplateAsync(CreateComponentOperationTemplateServiceModel createServiceModel);
        Task UpdateComponentOperationTemplateAsync(UpdateComponentOperationTemplateServiceModel updateServiceModel);
        Task DeleteComponentOperationTemplateAsync(string componentCode, int operationSequence);
    }
}
