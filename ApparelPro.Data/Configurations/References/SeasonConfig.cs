using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class SeasonConfig : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> entity)
        {
            entity.ToTable("Seasons");
            entity.HasKey(k => k.Id);

            entity.Property(p => p.Code)
                .HasMaxLength(10)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            // PurchaseOrder.Season references this by Code, not the surrogate Id.
            entity.HasIndex(p => p.Code).IsUnique();

            // Seeded directly from od_sea.dbf (8 records, confirmed against the existing
            // PurchaseOrders.Season values in use: FA95, FALL, Spring all resolve here).
            //entity.HasData(
            //    new Season { Id = 1, Code = "WINTER", Description = "WINTER" },
            //    new Season { Id = 2, Code = "SPRING", Description = "SPRING SEASON" },
            //    new Season { Id = 3, Code = "HOLIDA", Description = "HOLIDAY" },
            //    new Season { Id = 4, Code = "FALL", Description = "FALL" },
            //    new Season { Id = 5, Code = "FA93", Description = "FALL 1993" },
            //    new Season { Id = 6, Code = "FA95", Description = "FALL 1995" },
            //    new Season { Id = 7, Code = "SUMR96", Description = "SUMMER 1996" },
            //    new Season { Id = 8, Code = "SPRI96", Description = "SPRING SEASON 1996" }
            //);
        }
    }
}
