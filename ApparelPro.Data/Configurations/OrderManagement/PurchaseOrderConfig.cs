using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class PurchaseOrderConfig : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> entity)
        {
            entity.HasKey(k => new { k.BuyerCode, k.Order });

            entity.Property(p => p.BuyerCode)
               .ValueGeneratedNever()
               .IsRequired()
               .HasColumnType("int");

            entity.Property(p => p.Order)
              .ValueGeneratedNever()
              .HasMaxLength(12)
              .IsRequired()
              .HasColumnType("nvarchar");

            entity.Property(p => p.OrderDate)
              .HasColumnType("datetime");

            entity.Property(p => p.GarmentType)
              .HasColumnType("int");
              //.HasMaxLength(20);

            entity.Property(p => p.Description)
              .HasMaxLength(30)
              .HasColumnType("nvarchar")
              .IsRequired(false);

            // Fix (2026-08-16): both were nvarchar, matching Currency.Code/Unit.Code's old type.
            // Now that those narrowed to varchar(3), these narrow too so
            // FK_PurchaseOrders_Currencies_CurrencyCode / FK_PurchaseOrders_Units_UnitCode don't
            // hit a type mismatch.
            entity.Property(p => p.CurrencyCode)
              .HasMaxLength(3)
              .HasColumnType("varchar");

            entity.Property(p => p.UnitCode)
              .HasMaxLength(3)
              .HasColumnType("varchar");

            entity.Property(p => p.TotalQuantity)
              .HasColumnType("decimal(10,2)");

            entity.Property(p => p.CountryCode)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");

            entity.Property(p => p.Season)
              .HasMaxLength(10)
              .HasColumnType("nvarchar");

            entity.Property(p => p.BasisCode)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");

            entity.Property(p => p.BasisValue)
              .HasColumnType("decimal(10,2)");

            // Audit trail: who (email, from the JWT ClaimTypes.Name claim) and when overrode the
            // Total Quantity limit via the 'AllowOrderQuantityOverride' system parameter. Both
            // remain null unless an override has actually occurred for this order.
            entity.Property(p => p.QuantityOverriddenBy)
              .HasMaxLength(256)
              .HasColumnType("nvarchar")
              .IsRequired(false);

            entity.Property(p => p.QuantityOverriddenAt)
              .HasColumnType("datetime")
              .IsRequired(false);

            // Tier 1 relationships audit (2026-08-16): these 4 columns were previously enforced
            // only by matching values, with no real database constraint. BasisCode -> Basis.Code
            // is deliberately EXCLUDED here - 3 existing rows have a BasisCode with no matching
            // Basis row, so that one needs a data-cleanup decision before it can be added.
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(p => p.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(p => p.UnitCode)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(p => p.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<GarmentType>()
                .WithMany()
                .HasForeignKey(p => p.GarmentType)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
