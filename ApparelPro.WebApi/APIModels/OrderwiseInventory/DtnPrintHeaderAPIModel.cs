namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class DtnPrintHeaderAPIModel
    {
        public string DtnNumber { get; set; } = null!;
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
