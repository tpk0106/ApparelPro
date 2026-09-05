namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
