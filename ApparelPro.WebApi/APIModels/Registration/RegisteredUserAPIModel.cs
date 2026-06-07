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

        // Added properties to return to front-end client applications
        public Guid? AddressId { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? PostCode { get; set; }
        public string? CountryCode { get; set; }
    }
}
