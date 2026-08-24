using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.Data.Configurations.OrderManagement.Shipment
{
    public class CommercialInvoiceLineConfig : IEntityTypeConfiguration<CommercialInvoiceLine>
    {
        public void Configure(EntityTypeBuilder<CommercialInvoiceLine> entity)
        {
            entity.ToTable("CommercialInvoiceLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            // Legacy queries seek ie_coin2 by invo_no directly and, separately, by
            // buyer+order+type+style+new_order (see OD_SHPST.PRG) - both access paths
            // need a supporting index for report-time performance.
            entity.HasIndex(e => e.InvoiceNumber);
            entity.HasIndex(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.NewOrder });

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.Property(e => e.Order).HasColumnType("nvarchar(12)").IsRequired();
            entity.Property(e => e.StyleCode).HasColumnType("nvarchar(12)").IsRequired();
            entity.Property(e => e.NewOrder).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();

            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.Balance).HasColumnType("decimal(12,2)").IsRequired();

            entity.Property(e => e.QuotaCategory).HasColumnType("varchar(6)");
            entity.Property(e => e.FromYearMonth).HasColumnType("varchar(5)");
            entity.Property(e => e.ToYearMonth).HasColumnType("varchar(5)");
            entity.Property(e => e.QuotaCountry).HasColumnType("varchar(10)");
            entity.Property(e => e.PackingMedia).HasColumnType("varchar(1)");

            entity.HasOne<CommercialInvoiceHeader>()
                .WithMany()
                .HasForeignKey(e => e.InvoiceNumber)
                .HasPrincipalKey(h => h.InvoiceNumber)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);

            // Same Tier 2 relational pattern as PartShipmentConfig - ties each invoice
            // line back to the specific Style it was shipped against.
            entity.HasOne<Style>()
                .WithMany()
                .HasForeignKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .HasPrincipalKey(s => new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
