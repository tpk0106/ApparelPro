using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.Registration
{
    public class RegisterUserAPIModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;           
        public string? KnownAs { get; set; }       
        public string Gender { get; set; }        
        public DateTime? DateOfBirth { get; set; }        
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
