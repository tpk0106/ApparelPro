using ApparelPro.Data.Models.OrderManagement;
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

            entity.Property(p => p.CurrencyCode)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");

            entity.Property(p => p.UnitCode)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");

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
        }
    }
}
