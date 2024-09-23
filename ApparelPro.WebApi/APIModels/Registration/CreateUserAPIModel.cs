namespace ApparelPro.WebApi.APIModels.Registration
{
    public class CreateUserAPIModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; }
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
