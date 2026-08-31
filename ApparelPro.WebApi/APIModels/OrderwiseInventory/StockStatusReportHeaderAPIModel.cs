namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class StockStatusReportHeaderAPIModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TotalLineItems { get; set; }
    }
}
