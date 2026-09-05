namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DgnPrintHeaderServiceModel
    {
        public string DgnNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
