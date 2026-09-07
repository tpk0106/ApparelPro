using ApparelPro.Data.Models.Toolbar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Toolbar
{
    public class ToolbarPinConfig : IEntityTypeConfiguration<ToolbarPin>
    {
        public void Configure(EntityTypeBuilder<ToolbarPin> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserEmail)
                .IsRequired()
                .HasColumnType("nvarchar(256)");

            entity.Property(e => e.GroupKey)
                .IsRequired()
                .HasColumnType("nvarchar(64)");

            entity.Property(e => e.ItemRouterLink)
                .IsRequired()
                .HasColumnType("nvarchar(128)");

            entity.HasIndex(e => new { e.UserEmail, e.GroupKey, e.ItemRouterLink })
                .IsUnique();
        }
    }
}
