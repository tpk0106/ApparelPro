using ApparelPro.Data.Models.References;

namespace ApparelPro.WebApi.APIModels.Reference
{
    public class SupplierAPIModel
    {
        public int SupplierCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax { get; set; }
        public Guid? AddressId { get; set; } = Guid.Empty;        
        public ICollection<Address> Addresses { get; set; } = [];
    }
}
