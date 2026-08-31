namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockValuationReportHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public List<StockValuationReportStyleAPIModel> Styles { get; set; } = new();
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalIssuedValue { get; set; }
        public decimal TotalBalanceValue { get; set; }
    }

    public class StockValuationReportStyleAPIModel
    {
        public string StyleCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
    }
}
