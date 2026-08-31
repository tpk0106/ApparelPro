namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Value { get; set; }
        public decimal BalanceToReceive { get; set; }
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
    }
}
