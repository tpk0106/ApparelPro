using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Supplier
    {
        public int SupplierCode { get; set; }
        public Guid? AddressId { get; set; } = default;
        public string Name { get; set; } = string.Empty;
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax { get; set; }        

        // supplier has many addresses
        [NotMapped]
        public ICollection<Address> Addresses { get; set; } = [];
    }
}
