using apparelPro.BusinessLogic.Services.Models.OrderManagement.ITrimSheetReportService;

namespace apparelPro.BusinessLogic.Services
{
    public interface ITrimSheetReportService
    {
        // includeProfit is decided by the controller (based on the caller's role, mirroring
        // legacy's access('trimprof') gate) rather than by this service, so the same method
        // works for both "can see profit" and "cannot see profit" callers without duplicating
        // the rest of the calculation.
        Task<TrimSheetReportServiceModel> GetTrimSheetReportAsync(int buyerCode, string order, int typeCode, string styleCode, bool includeProfit);
    }
}
