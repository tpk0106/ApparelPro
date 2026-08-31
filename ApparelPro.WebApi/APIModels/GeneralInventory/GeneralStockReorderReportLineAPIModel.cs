namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockReorderReportLineAPIModel
    {
        public string ItemCode { get; set; } = null!;
        public string Description { get; set; } = "";
        public string Unit { get; set; } = null!;
        public decimal AveragePrice { get; set; }
        public decimal QtyInHand { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
    }
}
