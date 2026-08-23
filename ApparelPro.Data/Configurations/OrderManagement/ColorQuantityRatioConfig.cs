using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class ColorQuantityRatioConfig : IEntityTypeConfiguration<ColorQuantityRatio>
    {
        public void Configure(EntityTypeBuilder<ColorQuantityRatio> entity)
        {
            entity.ToTable("ColorQuantityRatios");

            entity.HasKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.Color });

            entity.Property(e => e.BuyerCode)
                .IsRequired()
                .HasColumnType("int")
                .HasColumnName("Buyer");

            // nvarchar to match Styles.Order/.Style - required for the FK to
            // Styles' natural key (SQL Server rejects an FK across differing
            // column types even at equal length - see the Tier 2 audit).
            entity.Property(e => e.Order)
                .IsRequired()
                .HasColumnType("nvarchar(12)")
                .HasColumnName("Order");

            entity.Property(e => e.TypeCode)
                .IsRequired()
                .HasColumnType("int")
                .HasColumnName("Type");

            entity.Property(e => e.StyleCode)
                .IsRequired()
                .HasColumnType("nvarchar(12)")
                .HasColumnName("Style");

            entity.Property(e => e.Color)
                .IsRequired()
                .HasColumnType("varchar(12)")
                .HasColumnName("Color");

            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasColumnType("varchar(30)")
                .HasColumnName("Description");

            entity.Property(e => e.Ratio)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasColumnName("Ratio");

            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasColumnType("decimal(12,2)")
                .HasColumnName("Quantity");

            entity.HasOne<Style>()
                .WithMany()
                .HasForeignKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .HasPrincipalKey(s => new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
