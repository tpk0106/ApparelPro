namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One outstanding PODetails line, joined with live OrderwiseStock context so the
    // frontend can render balances without a second round trip per row.
    public class GrnReceivableLineServiceModel
    {
        public int Buyer { get; set; }
        public string Order { get; set; } = null!;
        public int Type { get; set; }
        public string Style { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal OrderQuantity { get; set; }
        public decimal Balance { get; set; }    // outstanding qty not yet received
        public decimal QtyInHand { get; set; }  // current live stock balance, for context
    }
}
