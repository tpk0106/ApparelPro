namespace ApparelPro.WebApi.APIModels.OrderManagement
{
    public class SubContractAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = "";
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = "";
        public string SubContractorCode { get; set; } = "";
        public string SubContractorName { get; set; } = "";
        public decimal SubQuantity { get; set; }
        public decimal CostPerGarment { get; set; }
        public string Currency { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal ReceivedQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
    }
}
