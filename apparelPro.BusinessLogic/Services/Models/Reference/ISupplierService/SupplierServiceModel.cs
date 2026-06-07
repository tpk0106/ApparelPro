using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService
{
    public class SupplierServiceModel
    {
        public int SupplierCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax { get; set; }
        public Guid? AddressId { get; set; } = Guid.Empty;

        // supplier has many addresses
       public ICollection<Address> Addresses { get; set; } = [];
    }
}
