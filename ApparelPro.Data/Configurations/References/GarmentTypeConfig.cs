using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class GarmentTypeConfig : IEntityTypeConfiguration<GarmentType>
    {
        public void Configure(EntityTypeBuilder<GarmentType> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeName)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
