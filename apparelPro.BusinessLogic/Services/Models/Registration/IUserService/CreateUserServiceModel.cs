namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class CreateUserServiceModel
    {        
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public DateTime? DateOfBirth { get; set; }
        public string? KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastActive { get; set; } = DateTime.Now;
        public string? Gender { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public byte[]? Photo { get; set; }
    }
}
