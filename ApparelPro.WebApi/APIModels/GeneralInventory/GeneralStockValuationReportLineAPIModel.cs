namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockValuationReportLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal QtyInHand { get; set; }
        public decimal Value { get; set; }
        public decimal DamagedQuantity { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = null!;
    }
}
