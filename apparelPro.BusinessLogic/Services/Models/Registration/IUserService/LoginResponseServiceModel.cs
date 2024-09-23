namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class LoginResponseServiceModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; }
        public string? KnownAs { get; set; }
        public byte[] Photo { get; set; }

        public bool Success { get; set; }
    }
}
