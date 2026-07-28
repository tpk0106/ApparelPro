using ApparelPro.Data.Models.Registration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Registration
{
    public class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> entity)
        {
            entity.ToTable("RolePermissions");

            entity.Property(rp => rp.RoleId)
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            entity.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // One grant per role/permission pair - the admin screen replaces the
            // whole set for a role rather than allowing duplicate grants to pile up.
            entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();
        }
    }
}
