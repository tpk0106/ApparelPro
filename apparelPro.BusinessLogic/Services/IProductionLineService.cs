using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IProductionLineService
    {
        Task<PaginationResult<ProductionLineServiceModel>> GetProductionLinesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<ProductionLineServiceModel?> GetProductionLineByLineCodeAsync(string lineCode);
        Task<ProductionLineServiceModel> AddProductionLineAsync(CreateProductionLineServiceModel createServiceModel);
        Task UpdateProductionLineAsync(UpdateProductionLineServiceModel updateServiceModel);
        Task DeleteProductionLineAsync(string lineCode);
    }
}
