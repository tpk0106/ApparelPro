namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class GrnPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; } = null!;
    }
}
