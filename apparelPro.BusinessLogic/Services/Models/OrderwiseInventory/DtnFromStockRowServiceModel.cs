namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    // One row of the From Buyer/Order's stock, offered as the source-item picker for a
    // Direct Goods Transfer Note line.
    public class DtnFromStockRowServiceModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }
        public decimal MaxTransferableQuantity { get; set; }
    }
}
