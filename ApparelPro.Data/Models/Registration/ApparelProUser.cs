using ApparelPro.Data.Models.References;
using Microsoft.AspNetCore.Identity;

namespace ApparelPro.Data.Models.Registration
{
    public class ApparelProUser:IdentityUser
    {
        // this id is added as Id is coming from IdentityUser as a guid value
        // so it is required to make Id as int
      //  public int Id { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastActive { get; set; } = DateTime.Now;
        public Gender? Gender { get; set; } 
        //public string? City { get; set; }
        //public string? Country { get; set; }
        public Guid? AddressId { get; set; }
        // Navigation Property for easy EF Core joins
        public virtual Address? Address { get; set; }
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
