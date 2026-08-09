namespace apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService
{
    public class SaveSubContractServiceModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = null!;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = null!;
        public string SubContractorCode { get; set; } = null!;
        public decimal SubQuantity { get; set; }
        public decimal CostPerGarment { get; set; }
        public string Currency { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ReceivedQuantity { get; set; }
    }
}
