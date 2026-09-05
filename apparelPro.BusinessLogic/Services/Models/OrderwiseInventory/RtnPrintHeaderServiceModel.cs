namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class RtnPrintHeaderServiceModel
    {
        public string RtnNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
