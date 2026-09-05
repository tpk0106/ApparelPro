namespace apparelPro.BusinessLogic.Services.Models.OrderwiseInventory
{
    public class DtnHeaderServiceModel
    {
        public string DtnNumber { get; set; } = "";
        public int FromBuyerCode { get; set; }
        public string FromOrder { get; set; } = null!;
        public int ToBuyerCode { get; set; }
        public string ToOrder { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
