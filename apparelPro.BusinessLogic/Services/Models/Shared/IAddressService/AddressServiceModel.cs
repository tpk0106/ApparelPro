using ApparelPro.Data.Models.References;

namespace apparelPro.BusinessLogic.Services.Implementation.Shared
{
    public class AddressServiceModel
    {
        public int Id { get; set; }
        public Guid AddressId { get; set; }
        public AddressType? AddressType { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public int? PostCode { get; set; }
        public string? State { get; set; }
        public string? CountryCode { get; set; }
        public string Country { get; set; }
        public bool? Default { get; set; }
        public Buyer? Buyer { get; set; }
        public int? BuyerCode { get; set; }
    }
}
