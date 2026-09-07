using ApparelPro.Data.Models.Toolbar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Toolbar
{
    public class ToolbarPreferenceConfig : IEntityTypeConfiguration<ToolbarPreference>
    {
        public void Configure(EntityTypeBuilder<ToolbarPreference> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserEmail)
                .IsRequired()
                .HasColumnType("nvarchar(256)");

            entity.HasIndex(e => e.UserEmail)
                .IsUnique();
        }
    }
}
