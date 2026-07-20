namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinIssuableStrnLineAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal BalanceToReceive { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal StrnBalance { get; set; }
    }
}
