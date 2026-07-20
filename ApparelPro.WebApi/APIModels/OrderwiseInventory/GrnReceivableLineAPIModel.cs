namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnReceivableLineAPIModel
    {
        public int Buyer { get; set; }
        public string Order { get; set; } = null!;
        public int Type { get; set; }
        public string Style { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public decimal Balance { get; set; }
        public decimal QtyInHand { get; set; }
    }
}
