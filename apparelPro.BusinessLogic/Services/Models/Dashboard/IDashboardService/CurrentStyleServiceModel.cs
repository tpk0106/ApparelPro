namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class CurrentStyleServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;

        // "latest-entry" or "pinned" - drives the "Auto-picked from latest
        // floor entry" vs "Pinned in System Parameters" pill on the dashboard.
        public string Source { get; set; } = null!;
    }
}
