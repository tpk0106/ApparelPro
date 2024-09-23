using Microsoft.AspNetCore.Identity;

namespace ApparelPro.Data.Models.Registration
{
    public class ApparelProUser:IdentityUser
    {
        public DateTime? DateOfBirth { get; set; }
        public string? KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastActive { get; set; } = DateTime.Now;
        public Gender? Gender { get; set; } 
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? AddressId { get; set; }
        public byte[]? ProfilePhoto { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Transgender = 3
    }    
}
