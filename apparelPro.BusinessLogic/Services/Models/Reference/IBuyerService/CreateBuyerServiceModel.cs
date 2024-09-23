namespace apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService
{
    public class CreateBuyerServiceModel
    {
        public int BuyerCode { get; set; }
        public string? Status { get; set; }
        public Guid? AddreessId { get; set; } = Guid.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? AddressId { get; set; }
        public string? Fax { get; set; }
        public string? CUSDEC { get; set; }
    }
}
