namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class ArnLineItemAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public string AdditionalProcessCode { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
