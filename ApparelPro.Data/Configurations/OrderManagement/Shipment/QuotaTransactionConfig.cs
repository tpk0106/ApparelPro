using ApparelPro.Data.Models.OrderManagement.Shipments;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.Shipment
{
    public class QuotaTransactionConfig : IEntityTypeConfiguration<QuotaTransaction>
    {
        public void Configure(EntityTypeBuilder<QuotaTransaction> entity)
        {
            entity.ToTable("QuotaTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.Action).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.StyleCode).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.NewOrder).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.QuotaStatus).HasColumnType("varchar(1)").IsRequired();
            entity.Property(e => e.FromYearMonth).HasColumnType("varchar(5)").IsRequired();
            entity.Property(e => e.ToYearMonth).HasColumnType("varchar(5)").IsRequired();
            entity.Property(e => e.QuotaCountry).HasColumnType("varchar(10)").IsRequired();
            entity.Property(e => e.QuotaCategory).HasColumnType("varchar(20)").IsRequired();
            entity.Property(e => e.QuotaType).HasColumnType("varchar(2)").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();

            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").IsRequired();

            // Tier 1 relationships audit (2026-08-16): Unit was previously enforced only by
            // matching values against Units, with no real database constraint.
            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
