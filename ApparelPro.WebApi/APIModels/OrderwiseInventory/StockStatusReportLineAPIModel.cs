namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockStatusReportLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal BalanceToReceive { get; set; }
        public decimal DamagedQuantity { get; set; }
        public decimal QtyInHand { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
