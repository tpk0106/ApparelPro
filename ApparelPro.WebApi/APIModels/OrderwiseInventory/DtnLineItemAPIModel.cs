namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnLineItemAPIModel
    {
        public string FromItemCode { get; set; } = null!;
        public string ToItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
