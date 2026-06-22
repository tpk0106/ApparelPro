using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorSizeDetailsService;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Shared.Extensions;
using ApparelPro.WebApi.APIModels.OrderManagement;

namespace apparelPro.BusinessLogic.Services
{
    public interface IColorSizeBreakdownDetailsService
    {
        Task<PaginationResult<ColorSizeBreakdownDetailsServiceModel>> GetColorSizeDetailsAsync(int pageNumber, 
            int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<ColorSizeBreakdownDetailsServiceModel> AddColorSizeDetailsAsync(CreateColorSizeBreakdownDetailsServiceModel createColorSizeDetailsServiceModel);
        Task<bool> BulkSaveColorSizeDetailsAsync(int buyerCode, string order, int typeCode, string styleCode, List<CreateColorSizeBreakdownDetailsServiceModel> records);

        Task<List<ColorSizeBreakdownDetailsServiceModel>> GetBreakdownByStyleAsync(int buyer, string order, int type, string style);

        Task<StyleDimensionsLookupServiceModel> GetStyleDimensionsAsync(int buyerCode, string order, int typeCode, string styleCode);        

        // New: Fetches saved color/size matrix rows to hydrate the frontend layout grid
        Task<List<ColorSizeDetails>> GetSavedColorSizeMatrixAsync(int buyerCode, string order, int typeCode, string styleCode);
    }
}
