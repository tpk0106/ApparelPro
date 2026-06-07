using System.ComponentModel.DataAnnotations.Schema;

namespace ApparelPro.Data.Models.References
{
    public class Buyer
    {
        public int BuyerCode { get; set; }
        //public Guid? AddressId { get; set; } = default;
        public Guid? AddressId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;        
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax{ get; set; }
        public string? CUSDEC { get; set; }

        [NotMapped]
        // buyer has many addresses
        public List<Address>? Addresses { get; set; }
    }
}
