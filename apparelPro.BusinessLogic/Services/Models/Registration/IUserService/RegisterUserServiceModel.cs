using ApparelPro.Data.Models.Registration;

namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class RegisterUserServiceModel:ApparelProUser
    {        
        public string Token { get; set; }
        public string? KnownAs { get; set; }     
        public byte[]? Photo { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }        
    }
}
