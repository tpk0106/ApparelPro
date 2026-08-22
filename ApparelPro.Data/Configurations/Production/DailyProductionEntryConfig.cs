using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class DailyProductionEntryConfig : IEntityTypeConfiguration<DailyProductionEntry>
    {
        public void Configure(EntityTypeBuilder<DailyProductionEntry> entity)
        {
            entity.ToTable("DailyProductionEntries");

            entity.HasKey(e => new
            {
                e.Date,
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.LineCode,
                e.SectionCode
            });

            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.LineCode).HasColumnType("nvarchar(3)");
            entity.Property(e => e.SectionCode).HasColumnType("nvarchar(3)");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)");
            entity.Property(e => e.Hours).HasColumnType("decimal(4,1)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(9,1)");

            entity.HasOne<ProductionLine>()
                .WithMany()
                .HasForeignKey(e => e.LineCode)
                .HasPrincipalKey(l => l.LineCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Section>()
                .WithMany()
                .HasForeignKey(e => e.SectionCode)
                .HasPrincipalKey(s => s.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
