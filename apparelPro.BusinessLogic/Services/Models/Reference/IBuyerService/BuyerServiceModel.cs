using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService
{
    public class BuyerServiceModel
    {
        public int BuyerCode { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public Guid? AddressId { get; set; }
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax { get; set; }
        public string? CUSDEC { get; set; }
        public ICollection<Address>? Addresses { get; set; }
    }
    
}
