namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockMovementReportHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public int TotalLineItems { get; set; }
        public int FullyReceivedCount { get; set; }
        public int DamagedItemCount { get; set; }
        public int ShortfallCount { get; set; }
    }
}
