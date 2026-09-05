namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class GtnPrintHeaderServiceModel
    {
        public string GtnNumber { get; set; } = null!;
        public int FromBuyerCode { get; set; }
        public string FromBuyerName { get; set; } = null!;
        public string FromOrder { get; set; } = null!;
        public int ToBuyerCode { get; set; }
        public string ToBuyerName { get; set; } = null!;
        public string ToOrder { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
