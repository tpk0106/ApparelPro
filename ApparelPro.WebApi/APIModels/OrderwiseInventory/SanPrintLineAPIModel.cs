namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class SanPrintLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal AdjustedQuantity { get; set; }
        public string StoreCode { get; set; } = null!;
    }
}
