using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class ColorSizeDetailsConfig:IEntityTypeConfiguration<ColorSizeDetails>    
    {
        public void Configure(EntityTypeBuilder<ColorSizeDetails> entity)
        {
            // Define the 6-column Composite Primary Key
            entity.HasKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.Color, e.Size });

            // 3. Configure Properties with explicit column types, lengths, and precision
            entity.Property(e => e.BuyerCode)
                .IsRequired()
                .HasColumnType("int")
                .HasColumnName("Buyer");

            entity.Property(e => e.Order)
                .IsRequired()
                // nvarchar (not varchar) to match Styles.Order / PurchaseOrders.Order - required
                // for the FK to Styles' natural key added in the Tier 2 relationships audit
                // (SQL Server rejects an FK across differing column types even at equal length).
                .HasColumnType("nvarchar(12)")
                .HasColumnName("Order");

            entity.Property(e => e.TypeCode)
                .IsRequired()
                .HasColumnType("int")
                .HasColumnName("Type");

            entity.Property(e => e.StyleCode)
                .IsRequired()
                // nvarchar to match Styles.Style - see the Order property's comment above.
                .HasColumnType("nvarchar(12)")
                .HasColumnName("Style");

            entity.Property(e => e.Color)
                .IsRequired()
                .HasColumnType("varchar(12)")  // Replicates Clipper's 6-character length
                .HasColumnName("Color");

            entity.Property(e => e.Size)
                .IsRequired()
                .HasColumnType("varchar(12)") // Standard sizing length string threshold (Fixed typo here)
                .HasColumnName("Size");

            entity.Property(e => e.Ratio)
                .IsRequired()
                .HasColumnType("decimal(10,2)") // Added explicit decimal precision limits
                .HasColumnName("Ratio");

            entity.Property(e => e.Qty)
                .IsRequired()
                .HasColumnType("decimal(12,2)") // Larger scale capacity for piece totals
                .HasColumnName("Quantity");

            // FIXED (2026-08-07): free-text colour description/shade name,
            // matching the frontend "Colour Description / Shade Name" field's
            // maxLength of 30. Nullable - legacy/pre-existing rows have none.
            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasColumnType("varchar(30)")
                .HasColumnName("Description");

            // Tier 2 relationships audit: (BuyerCode, Order, TypeCode, StyleCode) was previously
            // enforced only by matching values against Styles' natural key, with no real
            // database constraint - a breakdown row could outlive its style.
            entity.HasOne<Style>()
                .WithMany()
                .HasForeignKey(e => new { e.BuyerCode, e.Order, e.TypeCode, e.StyleCode })
                .HasPrincipalKey(s => new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

