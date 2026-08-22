namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockMovementItemReportHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public int TransactionCount { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
