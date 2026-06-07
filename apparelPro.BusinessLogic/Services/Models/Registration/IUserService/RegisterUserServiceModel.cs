using ApparelPro.Data.Models.Registration;

namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class RegisterUserServiceModel:ApparelProUser
    {        
        public string Token { get; set; }
        public string Password { get; set; }
        public string? KnownAs { get; set; }     
        public byte[]? Photo { get; set; }       

        // Explicitly add address properties for service extraction
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public int? PostCode { get; set; }
        public string? State { get; set; }
        public string? CountryCode { get; set; }
        public int? AddressType { get; set; }
        public Boolean Default { get; set; } = true;
    }
}
