namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockLookupRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!; // The Basis code
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
    }
}
