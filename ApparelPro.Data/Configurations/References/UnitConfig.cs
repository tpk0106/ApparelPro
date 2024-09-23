using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class UnitConfig : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> entity)
        {
            entity.HasKey(k => k.Id);
            entity.Property(p => p.Code)
               .HasMaxLength(3)
               .IsRequired()
               .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
               .HasMaxLength(30)
               .IsRequired()
               .HasColumnType("nvarchar");         
        }
    }
}
