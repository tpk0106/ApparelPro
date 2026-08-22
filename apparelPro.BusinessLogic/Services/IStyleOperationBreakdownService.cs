using apparelPro.BusinessLogic.Services.Models.Production.IStyleOperationBreakdownService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IStyleOperationBreakdownService
    {
        Task<List<StyleOperationBreakdownServiceModel>> GetBreakdownByStyleAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        // Mirrors PR_OPD2.PRG's auto-copy-from-PR_MOP2 behaviour, made explicit
        // instead of an implicit side effect of viewing a component: only
        // seeds when the STYLE (not just this component) has zero existing
        // operation rows, exactly matching the legacy f_flg guard.
        Task<List<StyleOperationBreakdownServiceModel>> SeedFromTemplateAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            int componentSequence, string componentCode);

        Task<StyleOperationBreakdownSaveResultServiceModel> BulkSaveAndRecalculateAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            List<CreateStyleOperationBreakdownServiceModel> records);
    }
}
