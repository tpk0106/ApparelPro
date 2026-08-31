namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class GeneralStockSummaryReportLineAPIModel
    {
        public string StockTypeCode { get; set; } = null!;
        public string StockTypeDescription { get; set; } = "";
        public string StoreCode { get; set; } = "";
        public string StoreDescription { get; set; } = "";
        public decimal ValueInCurrency1 { get; set; }
        public decimal ValueInCurrency2 { get; set; }
        public string RowType { get; set; } = "Store";
    }
}
