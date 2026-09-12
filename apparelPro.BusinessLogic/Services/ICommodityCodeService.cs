using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommodityCodeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface ICommodityCodeService
    {
        Task<PaginationResult<CommodityCodeServiceModel>> GetCommodityCodesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<CommodityCodeServiceModel> GetCommodityCodeByCodeAsync(string code);
        Task<CommodityCodeServiceModel> AddCommodityCodeAsync(CreateCommodityCodeServiceModel createCommodityCodeServiceModel);
        Task UpdateCommodityCodeAsync(UpdateCommodityCodeServiceModel updateCommodityCodeServiceModel);
        Task DeleteCommodityCodeAsync(string code);
    }
}
