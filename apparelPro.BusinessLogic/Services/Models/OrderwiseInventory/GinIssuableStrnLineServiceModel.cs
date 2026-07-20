namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One outstanding STRN line, joined with live OrderwiseStock context so the
    // frontend can render balances without a second round trip per row.
    public class GinIssuableStrnLineServiceModel
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
