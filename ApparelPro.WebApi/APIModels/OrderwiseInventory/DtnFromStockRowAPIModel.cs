namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnFromStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal QtyInHand { get; set; }
        public decimal MaxTransferableQuantity { get; set; }
    }
}
