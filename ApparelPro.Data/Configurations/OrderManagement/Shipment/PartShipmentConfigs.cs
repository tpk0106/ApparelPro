using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
namespace ApparelPro.Data.Configurations.OrderManagement.Shipment
{
    public class PartShipmentConfig: IEntityTypeConfiguration<PartShipment>
    {
        public void Configure(EntityTypeBuilder<PartShipment> entity)
        {
            entity.ToTable("PartShipments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            // Setup a strict composite constraint index to enable high-speed historical tracking seeks
            entity.HasIndex(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.NewOrder, e.DestinationCode, e.ShipDate }).IsUnique();

            // nvarchar (not varchar) to match Styles.Order / Styles.Style - required for the FK
            // to Styles' natural key added in the Tier 2 relationships audit (SQL Server rejects
            // an FK across differing column types even at equal length).
            entity.Property(e => e.Order).HasColumnType("nvarchar(12)").IsRequired();
            entity.Property(e => e.StyleCode).HasColumnType("nvarchar(12)").IsRequired();
            entity.Property(e => e.NewOrder).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.DestinationCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.SubContractFlag).HasColumnType("varchar(1)").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.ShippingMode).HasColumnType("varchar(3)").IsRequired();

            entity.Property(e => e.QuotaCountry).HasColumnType("varchar(10)");
            entity.Property(e => e.QuotaStatus).HasColumnType("varchar(1)");
            entity.Property(e => e.QuotaCategory).HasColumnType("varchar(6)");
            entity.Property(e => e.QuotaType).HasColumnType("varchar(2)");
            entity.Property(e => e.FromYearMonth).HasColumnType("varchar(5)");
            entity.Property(e => e.ToYearMonth).HasColumnType("varchar(5)");

            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.Balance).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.ShipDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.OrderDate).HasColumnType("date").IsRequired();

            // Tier 1 relationships audit (2026-08-16): Unit was previously enforced only by
            // matching values against Units, with no real database constraint.
            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);

            // Tier 2 relationships audit: (BuyerCode, Order, TypeCode, StyleCode) was previously
            // enforced only by matching values against Styles' natural key, with no real
            // database constraint.
            entity.HasOne<Style>()
                .WithMany()
                .HasForeignKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .HasPrincipalKey(s => new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode })
                .OnDelete(DeleteBehavior.Restrict);

            // Part B fix: DestinationCode was previously free text with no master data behind
            // it at all (Destinations had no business-code column to reference). Destination.Code
            // now exists with a unique constraint, so this can be a real FK.
            entity.HasOne<Destination>()
                .WithMany()
                .HasForeignKey(e => e.DestinationCode)
                .HasPrincipalKey(d => d.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
