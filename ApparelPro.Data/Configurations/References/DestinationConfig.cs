using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class DestinationConfig : IEntityTypeConfiguration<Destination>
    {
        public void Configure(EntityTypeBuilder<Destination> entity)
        {
            // Part B fix: legacy od_dest is keyed on cont_cd+dest_cd (dest_cd = 3-char business
            // code, entered via pict '!!!' in od_dest1.prg). The modern schema had replaced that
            // with a meaningless surrogate Id and never stored dest_cd at all, so nothing could
            // ever FK against a real destination code. Natural key restored here to match legacy.
            entity.Property(p => p.Code)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("varchar");

            entity.HasKey(x => new { x.CountryCode, x.Code });

            entity.Property(p => p.CountryCode)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.DestinationName)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            // PartShipments.DestinationCode carries only the 3-char Code, with no CountryCode
            // context (confirmed by reading PartShipmentConfigs.cs and the frontend's "Dest Code"
            // field) - the frontend already treats these codes as globally unique, so a unique
            // constraint on Code alone is required to support that FK.
            entity.HasIndex(p => p.Code).IsUnique();

            // Tier 2 relationships audit: CountryCode was previously enforced only by matching
            // values against Countries, with no real database constraint - likely an oversight,
            // since every sibling reference table (Currency, PurchaseOrder) already has this FK.
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(p => p.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Seeds the 2 destination codes ("BAL"/"LON") already in live use on PartShipments
            // rows, which had no master data behind them at all before this fix - required
            // before PartShipments.DestinationCode -> Destinations.Code can be enforced as a
            // real FK. Confirmed with the user (2026-08-23): BAL = Baltimore, USA; LON = London,
            // United Kingdom (GB row added in CountryConfig.cs for this).
            //entity.HasData(
            //    new Destination { CountryCode = "USA", Code = "BAL", DestinationName = "Baltimore" },
            //    new Destination { CountryCode = "GB", Code = "LON", DestinationName = "London" }
            //);
        }
    }
}
