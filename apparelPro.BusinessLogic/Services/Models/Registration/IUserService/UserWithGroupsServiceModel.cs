namespace apparelPro.BusinessLogic.Services.Models.Registration.IUserService
{
    public class UserWithGroupsServiceModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? KnownAs { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Groups { get; set; } = new();
    }
}
