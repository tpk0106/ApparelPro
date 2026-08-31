namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockSummaryReportLineAPIModel
    {
        public string StockTypeCode { get; set; } = null!;
        public string StockTypeDescription { get; set; } = "";
        public string StoreCode { get; set; } = "";
        public decimal ValueInCurrency1 { get; set; }
        public decimal ValueInCurrency2 { get; set; }
        public string RowType { get; set; } = "Store";
    }
}
