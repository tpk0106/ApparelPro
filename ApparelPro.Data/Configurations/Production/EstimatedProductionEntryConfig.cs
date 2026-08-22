using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class EstimatedProductionEntryConfig : IEntityTypeConfiguration<EstimatedProductionEntry>
    {
        public void Configure(EntityTypeBuilder<EstimatedProductionEntry> entity)
        {
            entity.ToTable("EstimatedProductionEntries");

            entity.HasKey(e => new
            {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.LineCode,
                e.Date
            });

            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.LineCode).HasColumnType("nvarchar(3)");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(9,1)");

            entity.HasOne<ProductionLine>()
                .WithMany()
                .HasForeignKey(e => e.LineCode)
                .HasPrincipalKey(l => l.LineCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
