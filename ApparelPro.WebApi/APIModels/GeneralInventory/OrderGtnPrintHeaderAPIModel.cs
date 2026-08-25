namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class OrderGtnPrintHeaderAPIModel
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
