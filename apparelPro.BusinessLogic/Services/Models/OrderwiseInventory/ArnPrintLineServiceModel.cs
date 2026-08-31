namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class ArnPrintLineServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Value { get; set; }

        // Legacy IN_ARN2.PRG computes this as "qty_in_hd - to_dt_rec" off in_stock,
        // which doesn't match its own "Bal. to Receive" column header (that formula
        // mixes a physical on-hand quantity with a cumulative received total, not a
        // receivable balance). This instead shows the actual outstanding
        // Issued-but-not-yet-Received balance (ToDateIssued - ToDateReceived, current
        // OrderwiseStock state, i.e. after this ARN's own effect), matching what
        // AdditionalGoodsReceiptNoteService's own entry-time cap already checks.
        public decimal BalanceToReceive { get; set; }

        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
    }
}
