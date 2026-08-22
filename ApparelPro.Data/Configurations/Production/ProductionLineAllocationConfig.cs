using ApparelPro.Data.Models.Production;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class ProductionLineAllocationConfig : IEntityTypeConfiguration<ProductionLineAllocation>
    {
        public void Configure(EntityTypeBuilder<ProductionLineAllocation> entity)
        {
            entity.ToTable("ProductionLineAllocations");

            entity.HasKey(e => new
            {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.ShipmentOrder,
                e.LineCode
            });

            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.ShipmentOrder).HasColumnType("varchar(12)");
            entity.Property(e => e.LineCode).HasColumnType("nvarchar(3)"); // must match ProductionLines.LineCode (nvarchar)
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.CostPerDay).HasColumnType("decimal(10,3)");
            entity.Property(e => e.TotalQuantity).HasColumnType("decimal(12,2)");
            entity.Property(e => e.NumberOfDays).HasColumnType("decimal(6,1)");
            entity.Property(e => e.LeadTimeDays).HasColumnType("decimal(5,1)");
            entity.Property(e => e.EstimatedProductionPerDay).HasColumnType("decimal(9,0)");

            entity.HasOne<ProductionLine>()
                .WithMany()
                .HasForeignKey(e => e.LineCode)
                .HasPrincipalKey(l => l.LineCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(e => e.CurrencyCode)
                .HasPrincipalKey(c => c.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
