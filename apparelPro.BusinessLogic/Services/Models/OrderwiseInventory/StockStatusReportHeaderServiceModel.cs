namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class StockStatusReportHeaderServiceModel
    {
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public int TotalLineItems { get; set; }
    }
}
