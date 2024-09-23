using ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Models.Registration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.Data
{
    public class UserIdentityDbContext: IdentityDbContext<ApparelProUser>
    {
        public UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options):base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ApparelProUserConfig());
        }
    }
}
