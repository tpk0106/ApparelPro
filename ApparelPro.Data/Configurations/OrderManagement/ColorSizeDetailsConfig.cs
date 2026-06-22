using ApparelPro.Data.Models.OrderManagement;
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
                .HasColumnType("varchar(12)") // Standard order length constraint
                .HasColumnName("Order");

            entity.Property(e => e.TypeCode)
                .IsRequired()
                .HasColumnType("int")
                .HasColumnName("Type");

            entity.Property(e => e.StyleCode)
                .IsRequired()
                .HasColumnType("varchar(12)") // Matches your StyleCode type length
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
        }
    }
}

