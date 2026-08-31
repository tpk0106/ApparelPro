namespace ApparelPro.WebApi.APIModels.OrderwiseInventory
{
    public class AinPrintHeaderAPIModel
    {
        public string AinNumber { get; set; } = null!;
        public int BuyerCode { get; set; }
        public string BuyerName { get; set; } = null!;
        public string Order { get; set; } = null!;
        public string SubContractorCode { get; set; } = null!;
        public string AdditionalProcessCode { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public DateTime PrintedOn { get; set; }
    }
}
