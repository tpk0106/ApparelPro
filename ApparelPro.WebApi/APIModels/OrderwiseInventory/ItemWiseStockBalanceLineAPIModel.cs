namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ItemWiseStockBalanceLineAPIModel
    {
        public string StockTypeCode { get; set; } = "";
        public string StockTypeDescription { get; set; } = "";
        public string ItemGroupCode { get; set; } = "";
        public string ItemGroupDescription { get; set; } = "";
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal BalanceValue { get; set; }
        public string RowType { get; set; } = "Item";
    }
}
