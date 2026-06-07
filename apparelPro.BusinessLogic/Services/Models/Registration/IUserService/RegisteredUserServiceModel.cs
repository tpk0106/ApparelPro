using ApparelPro.Data.Models.Registration;

namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class RegisteredUserServiceModel:ApparelProUser
    {       
        public string Token { get; set; }
        public string? KnownAs { get; set; }
        public byte[]? Photo { get; set; } = null;

        // Match Address fields for cross-context service layer mapping
        public Guid? AddressId { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? PostCode { get; set; }
        public string? CountryCode { get; set; }
    }
}
