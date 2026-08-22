using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class SectionConfig : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> entity)
        {
            entity.ToTable("Sections");
            entity.HasKey(x => x.Code);
            entity.Property(p => p.Code)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            // Seeded 1:1 from the legacy OD_SECT.DBF data (6 records, read
            // directly from the binary DBF - no CSV export existed).
            entity.HasData(
                new Section { Id = 1, Code = "001", Description = "CUTTING", IsFinal = false },
                new Section { Id = 2, Code = "002", Description = "SEWING", IsFinal = false },
                new Section { Id = 3, Code = "003", Description = "CHECKING", IsFinal = false },
                new Section { Id = 4, Code = "004", Description = "FINISHING", IsFinal = false },
                new Section { Id = 5, Code = "005", Description = "PACKING", IsFinal = true },
                new Section { Id = 6, Code = "006", Description = "SHIPPING", IsFinal = false }
            );
        }
    }
}
