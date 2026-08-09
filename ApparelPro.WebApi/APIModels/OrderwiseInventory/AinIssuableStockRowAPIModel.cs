namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinIssuableStockRowAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string StoreCode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal OrderedQuantity { get; set; }
        public decimal ShadowBalance { get; set; }
        public decimal ToDateIssued { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal AvailableForIssue { get; set; }
    }
}
