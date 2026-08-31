namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockValuationReportHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public List<StockValuationReportStyleServiceModel> Styles { get; set; } = new();
        public decimal TotalReceivedValue { get; set; }
        public decimal TotalIssuedValue { get; set; }
        public decimal TotalBalanceValue { get; set; }
    }

    public class StockValuationReportStyleServiceModel
    {
        public string StyleCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal UnitPrice { get; set; }
    }
}
