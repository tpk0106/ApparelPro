namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class OrderwiseInventorySummaryAPIModel
    {
        public int TotalLineItems { get; set; }
        public int FullyReceivedCount { get; set; }
        public int DamagedItemCount { get; set; }
        public int ShortfallCount { get; set; }
        public List<StockItemMovementAPIModel> Items { get; set; } = new();
    }
}
