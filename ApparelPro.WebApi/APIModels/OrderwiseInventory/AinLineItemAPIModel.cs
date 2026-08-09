namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinLineItemAPIModel
    {
        public string StoreCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
