using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;

namespace ApparelPro.Data.Configurations.OrderManagement.Shipment
{
    public class CommercialInvoiceHeaderConfig : IEntityTypeConfiguration<CommercialInvoiceHeader>
    {
        public void Configure(EntityTypeBuilder<CommercialInvoiceHeader> entity)
        {
            entity.ToTable("CommercialInvoiceHeaders");
            entity.HasKey(e => e.InvoiceNumber);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.Property(e => e.InvoiceDate).HasColumnType("date").IsRequired();

            entity.Property(e => e.DocumentaryBuyerCode).HasColumnType("varchar(6)");
            entity.Property(e => e.NotifyPartyCode).HasColumnType("varchar(6)");
            entity.Property(e => e.ConsigneeCode).HasColumnType("varchar(6)");

            entity.Property(e => e.LoadPortCode).HasColumnType("varchar(3)");
            entity.Property(e => e.DestinationCode).HasColumnType("varchar(3)");
            entity.Property(e => e.ContinuingDestinationCode).HasColumnType("varchar(3)");
            entity.Property(e => e.CarrierCode).HasColumnType("varchar(6)");
            entity.Property(e => e.ShipDate).HasColumnType("date");

            entity.Property(e => e.LcNumber).HasColumnType("varchar(20)");
            entity.Property(e => e.LcDate).HasColumnType("date");
            entity.Property(e => e.AssessmentNumber).HasColumnType("varchar(10)");
            entity.Property(e => e.IssuingBankCode).HasColumnType("varchar(3)");

            entity.Property(e => e.Remark1).HasColumnType("varchar(40)");
            entity.Property(e => e.Remark2).HasColumnType("varchar(40)");
            entity.Property(e => e.Remark3).HasColumnType("varchar(40)");
            entity.Property(e => e.Detail).HasColumnType("nvarchar(max)"); // legacy memo (.DBT) field

            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)");
            entity.Property(e => e.TradeTermCode).HasColumnType("varchar(3)");
            entity.Property(e => e.TradeTermLine1).HasColumnType("varchar(30)");
            entity.Property(e => e.TradeTermLine2).HasColumnType("varchar(30)");
            entity.Property(e => e.TradeTermLine3).HasColumnType("varchar(30)");

            // Real relational constraint replacing legacy's unenforced free-text buyer code.
            entity.HasOne<Buyer>()
                .WithMany()
                .HasForeignKey(e => e.BuyerCode)
                .HasPrincipalKey(b => b.BuyerCode)
                .OnDelete(DeleteBehavior.Restrict);

            // NOT a real FK to Destinations (unlike PartShipment.DestinationCode): historical
            // invoice headers reference destination codes (e.g. SAV, LON, ELD) going back to
            // 1994 that the current Destinations master data doesn't carry - confirmed via a
            // full import attempt where the FK rejected the majority of real historical rows.
            // Kept as free text, same as NotifyPartyCode/ConsigneeCode/CarrierCode.
        }
    }
}
