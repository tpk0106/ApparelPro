namespace apparelPro.BusinessLogic.Services.Models.GeneralInventory
{
    public class OrderGtnPrintHeaderServiceModel
    {
        public string OgtnNumber { get; set; } = null!;
        public string Direction { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = "";
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
