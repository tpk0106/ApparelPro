using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class BuyerAPIModel
    {
        public int BuyerCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax { get; set; }
        public string? CUSDEC { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
