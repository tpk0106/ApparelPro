using apparelPro.BusinessLogic.Services.Models.Production.IStyleComponentBreakdownService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStyleComponentBreakdownService
    {
        Task<List<StyleComponentBreakdownServiceModel>> GetBreakdownByStyleAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        Task<bool> BulkSaveComponentBreakdownAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            List<CreateStyleComponentBreakdownServiceModel> records);
    }
}
