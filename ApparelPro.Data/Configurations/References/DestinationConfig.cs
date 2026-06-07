using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class DestinationConfig : IEntityTypeConfiguration<Destination>
    {
        public void Configure(EntityTypeBuilder<Destination> entity)
        {
            entity.Property(p=>p.Id)
                .IsRequired()
                .UseIdentityColumn();

            entity.HasKey(x => new { x.Id, x.CountryCode });

            entity.Property(p => p.CountryCode)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.DestinationName)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
