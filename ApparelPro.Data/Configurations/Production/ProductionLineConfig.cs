using ApparelPro.Data.Models.Production;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class ProductionLineConfig : IEntityTypeConfiguration<ProductionLine>
    {
        public void Configure(EntityTypeBuilder<ProductionLine> entity)
        {
            entity.ToTable("ProductionLines");
            entity.HasKey(x => x.LineCode);
            entity.Property(p => p.LineCode)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            // Fix (2026-08-16): was nvarchar, matching Currency.Code/Unit.Code's old type. Now
            // that those are varchar(3), these narrow too so the existing FK_ProductionLines_*
            // constraints keep working - see AddTier1ReferenceDataForeignKeys migration.
            entity.Property(p => p.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("varchar");

            entity.Property(p => p.UnitCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("varchar");

            entity.Property(p => p.LineCostPerDay)
                .HasColumnType("decimal(10,3)");

            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(p => p.CurrencyCode)
                .HasPrincipalKey(c => c.Code)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(p => p.UnitCode)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
