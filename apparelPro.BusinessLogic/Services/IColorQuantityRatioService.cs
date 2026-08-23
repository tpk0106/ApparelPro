using apparelPro.BusinessLogic.Services.Models.OrderManagement.IColorQuantityRatioService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IColorQuantityRatioService
    {
        Task<List<ColorQuantityRatioServiceModel>> GetByStyleAsync(int buyerCode, string order, int typeCode, string styleCode);

        Task<bool> BulkSaveColorQuantityRatiosAsync(int buyerCode, string order, int typeCode, string styleCode,
            List<CreateColorQuantityRatioServiceModel> records);

        // Persists the user's Ratio/Quantity choice for a style - Colour stage
        // (Style.ColorRatio) and Size stage (Style.SizeRatio) are set
        // independently, matching legacy od_style's separate c_rt/s_rt flags.
        Task SetColorRatioModeAsync(int buyerCode, string order, int typeCode, string styleCode, string mode);
        Task SetSizeRatioModeAsync(int buyerCode, string order, int typeCode, string styleCode, string mode);
    }
}
