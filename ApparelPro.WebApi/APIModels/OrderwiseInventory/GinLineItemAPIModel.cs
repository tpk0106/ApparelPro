namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GinLineItemAPIModel
    {
        public string StockCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
