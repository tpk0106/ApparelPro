using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class BuyerAPIModel
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
