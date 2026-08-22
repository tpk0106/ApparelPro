using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class GarmentComponentConfig : IEntityTypeConfiguration<GarmentComponent>
    {
        public void Configure(EntityTypeBuilder<GarmentComponent> entity)
        {
            entity.ToTable("GarmentComponents");
            entity.HasKey(x => x.ComponentCode);
            entity.Property(p => p.ComponentCode)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar");
        }
    }
}
