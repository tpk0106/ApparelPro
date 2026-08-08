using apparelPro.BusinessLogic.Services.Models.Reference.IAdditionalCostService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services.interfaces.Reference
{
    public interface IAdditionalCostService
    {
        Task<PaginationResult<AdditionalCostServiceModel>> GetAdditionalCostsAsync(
            int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<AdditionalCostServiceModel?> GetAdditionalCostByCodeAsync(string code);
        Task<bool> DoesAdditionalCostExistAsync(string code);
        Task<AdditionalCostServiceModel> AddAdditionalCostAsync(CreateAdditionalCostServiceModel createAdditionalCostServiceModel);
        Task UpdateAdditionalCostAsync(UpdateAdditionalCostServiceModel updateAdditionalCostServiceModel);
        Task DeleteAdditionalCostAsync(string code);
    }
}
