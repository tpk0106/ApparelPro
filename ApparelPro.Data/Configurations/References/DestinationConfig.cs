using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class DestinationConfig : IEntityTypeConfiguration<Destination>
    {
        public void Configure(EntityTypeBuilder<Destination> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(p => p.Code)
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
