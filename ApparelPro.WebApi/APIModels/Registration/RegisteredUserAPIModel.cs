namespace ApparelPro.WebApi.APIModels.Registration
{
    public class RegisteredUserAPIModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; }
        public string? KnownAs { get; set; } 
        public byte[] Photo { get; set; }

        public bool Success { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
