namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class OrderPipelineResultServiceModel
    {
        public List<OrderPipelineRowServiceModel> Items { get; set; } = new();
        public int TotalItems { get; set; }

        // Counts across every running (not-yet-complete) style, independent
        // of whatever page/filter was requested - drives the KPI tiles.
        // Index matches Stage (0-6).
        public int[] StageCounts { get; set; } = new int[7];

        // Count of styles where IsOverdue is true, across ALL running styles
        // (not just the current page/filter) - drives the "N orders overdue"
        // attention banner.
        public int OverdueCount { get; set; }
    }
}
