using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CountryConfig : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> entity)
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");

            entity.Property(e => e.Id)
                .UseIdentityColumn();

            entity.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(30)
               .HasColumnType("nvarchar");

            entity.Property(p => p.Flag)
                .HasColumnType("varbinary(MAX)");

            // Tier 2 relationships audit (Part B): GB was missing entirely, blocking a real
            // Destinations row for "LON" (London) - PartShipments already had a live "LON"
            // DestinationCode with nothing behind it. Id 50 chosen as the next free value
            // (existing rows top out at 49).
            //entity.HasData(
            //    new Country { Id = 50, Code = "GB", Name = "United Kingdom" },
            //     new Country { Code = "USA", Id = 51, Name = "United States" }
            //);
        }
    }
}
