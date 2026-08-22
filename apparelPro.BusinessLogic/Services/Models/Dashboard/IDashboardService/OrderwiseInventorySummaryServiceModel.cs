namespace apparelPro.BusinessLogic.Services.Models.Dashboard.IDashboardService
{
    public class OrderwiseInventorySummaryServiceModel
    {
        public int TotalLineItems { get; set; }
        public int FullyReceivedCount { get; set; }
        public int DamagedItemCount { get; set; }
        public int ShortfallCount { get; set; }
        public List<StockItemMovementServiceModel> Items { get; set; } = new();
    }
}
