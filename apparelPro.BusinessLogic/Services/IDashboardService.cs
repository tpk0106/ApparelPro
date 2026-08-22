using apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDashboardService
    {
        Task<CurrentStyleServiceModel?> GetCurrentStyleAsync();

        Task<ProductionProgressServiceModel> GetProductionProgressAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        // Defaults sectionCode to the contract section (Cutting) when not
        // given - that's the series the dashboard actually charts.
        Task<List<DailyTrendPointServiceModel>> GetDailyTrendAsync(
            int buyerCode, string order, int typeCode, string styleCode,
            string? sectionCode, int days);

        // Every section's daily quantity, aligned to the same trailing set
        // of dates (the most recent `days` distinct dates with any entry
        // across ANY section, not per-section) so they can be charted
        // together as one multi-line series.
        Task<List<DailyTrendSeriesServiceModel>> GetDailyTrendAllSectionsAsync(
            int buyerCode, string order, int typeCode, string styleCode, int days);

        Task<OrderManagementSummaryServiceModel?> GetOrderManagementSummaryAsync(
            int buyerCode, string order, int typeCode, string styleCode);

        // Stock is tracked per Buyer+Order, not per Type+Style - fabric/trims
        // on an order are shared across every style cut from it, so this is
        // deliberately not scoped any narrower than the other three params
        // suggest (kept as buyerCode+order only, matching how
        // IStockMovementReportService itself is scoped).
        Task<OrderwiseInventorySummaryServiceModel?> GetOrderwiseInventorySummaryAsync(
            int buyerCode, string order);
    }
}
