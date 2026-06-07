
namespace ApparelPro.WebApi.APIModels.Registration
{
    public class RegisterUserAPIModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;           
        public string? KnownAs { get; set; }       
        public string Gender { get; set; }        
        public string? phoneNumber { get; set; }        
        public DateTime? DateOfBirth { get; set; }
        
        // Nested Address Fields
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public int? PostCode { get; set; }
        public string? State { get; set; }
        public string? CountryCode { get; set; }
        public int? AddressType { get; set; }
        public Boolean Default { get; set; } = true;
    }
}
