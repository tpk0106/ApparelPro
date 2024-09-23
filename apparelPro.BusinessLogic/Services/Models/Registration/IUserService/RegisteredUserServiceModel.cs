using ApparelPro.Data.Models.Registration;

namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class RegisteredUserServiceModel:ApparelProUser
    {       
        public string Token { get; set; }
        public string? KnownAs { get; set; }
        public byte[]? Photo { get; set; }
    }
}
