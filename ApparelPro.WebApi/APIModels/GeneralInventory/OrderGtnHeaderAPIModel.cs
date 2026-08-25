namespace ApparelPro.WebApi.APIModels.GeneralInventory
{
    public class OrderGtnHeaderAPIModel
    {
        public string OgtnNumber { get; set; } = null!;
        public string Direction { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
    }
}
