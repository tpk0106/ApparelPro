namespace ApparelPro.Data.Models.References
{
    public class Buyer
    {
        public int BuyerCode { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; } 
        public Guid? AddressId { get; set; }        
        public string? TelephoneNos { get; set; }
        public string? MobileNos { get; set; }
        public string? Fax{ get; set; }
        public string? CUSDEC { get; set; }

        // buyer has many addresses
        public ICollection<Address>? Addresses { get; set; }
    }
}
