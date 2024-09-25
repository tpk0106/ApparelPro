using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class UpdateBuyerAPIModel
    {
        public int BuyerCode { get; set; }
        public string? Status { get; set; }
        public Guid? AddressId { get; set; } 
        public string? Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }        
        public string? Fax { get; set; }
        public string? CUSDEC { get; set; }        
    }
}
