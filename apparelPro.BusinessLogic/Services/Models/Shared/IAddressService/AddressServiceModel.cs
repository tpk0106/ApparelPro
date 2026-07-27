using ApparelPro.Data.Models.References;
using System.ComponentModel.DataAnnotations.Schema;

namespace apparelPro.BusinessLogic.Services.Implementation.Shared
{
    public class AddressServiceModel
    {
        public int Id { get; set; }
        public Guid AddressId { get; set; }
        public AddressType? AddressType { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? PostCode { get; set; }
        public string? State { get; set; }
        public string? CountryCode { get; set; }
        [NotMapped]
        public string Country { get; set; }
        public bool? Default { get; set; }
        //  [NotMapped]
        // public Buyer? Buyer { get; set; }
        [NotMapped]
        public int? BuyerCode { get; set; }
    }
}
