using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class EstimatedProductionLineAllocationConfig : IEntityTypeConfiguration<EstimatedProductionLineAllocation>
    {
        public void Configure(EntityTypeBuilder<EstimatedProductionLineAllocation> entity)
        {
            entity.ToTable("EstimatedProductionLineAllocations");

            entity.HasKey(e => new { e.BuyerCode, e.StyleCode });

            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.LineCode).HasColumnType("nvarchar(3)"); // must match ProductionLines.LineCode (nvarchar)
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.TotalQuantity).HasColumnType("decimal(12,2)");
            entity.Property(e => e.NumberOfDays).HasColumnType("decimal(6,1)");
            entity.Property(e => e.LeadTimeDays).HasColumnType("decimal(5,1)");
            entity.Property(e => e.EstimatedProductionPerDay).HasColumnType("decimal(9,0)");

            entity.HasOne<ProductionLine>()
                .WithMany()
                .HasForeignKey(e => e.LineCode)
                .HasPrincipalKey(l => l.LineCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
