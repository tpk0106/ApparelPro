using ApparelPro.Data.Configurations.References.ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Configurations.Registration;
using ApparelPro.Data.Models.Registration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ApparelPro.Data
{
    public class UserIdentityDbContext: IdentityDbContext<ApparelProUser>
    {
        // Add this temporary block inside your UserIdentityDbContext class:
        public UserIdentityDbContext()
        {
        }

        public UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options):base(options)
        {            
        }

        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Points directly to your local SQL Express server using Windows Authentication
                optionsBuilder.UseSqlServer("Server=THUSITHPC\\SQLEXPRESS;Database=ApparelPro;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        // This forces an offline creation channel that completely bypasses your SQL Express server permission locks
        //        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ApparelProTemp;Trusted_Connection=True;TrustServerCertificate=True;");
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Load the user profile definitions
            builder.ApplyConfiguration(new ApparelProUserConfig());            

            // 2. Load the pluralization tracking definition to clear the mismatch
            builder.ApplyConfiguration(new IdentityAddressConfig());

            // 3. Load the Permission / RolePermission catalog definitions (Stage 2
            // of the access-control rework - see Authorization/AccessPolicies.cs
            // for Stage 1's role-string constants).
            builder.ApplyConfiguration(new PermissionConfig());
            builder.ApplyConfiguration(new RolePermissionConfig());
        }
    }
}
