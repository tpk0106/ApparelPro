using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class FeatureConfig : IEntityTypeConfiguration<Feature>
    {
        public void Configure(EntityTypeBuilder<Feature> entity)
        {
            entity.HasKey(k => k.Id);
            entity.Property(p => p.Code)
               .HasMaxLength(3)
              // .IsRequired()
               .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
              .HasMaxLength(20)              
              .HasColumnType("nvarchar");

        }
    }
}
