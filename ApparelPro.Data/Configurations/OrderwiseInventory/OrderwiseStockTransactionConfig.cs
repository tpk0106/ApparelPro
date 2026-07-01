
using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class OrderwiseStockTransactionConfig:IEntityTypeConfiguration<OrderwiseStockTransaction>
    {
        public void Configure(EntityTypeBuilder<OrderwiseStockTransaction> entity)
        {
            entity.ToTable("OrderwiseStockTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            // Unique index compound constraint to track exact transaction lines cleanly
            entity.HasIndex(e => new { e.DocumentNumber, e.TransactionType, e.StockCode, e.ItemCode }).IsUnique();

            entity.Property(e => e.DocumentNumber).HasColumnType("varchar(10)").IsRequired();
            entity.Property(e => e.TransactionType).HasColumnType("varchar(2)").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.DepartmentCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.CreatedByUsername).HasColumnType("varchar(20)").IsRequired();
            entity.Property(e => e.TransactionDate).HasColumnType("date").IsRequired();
        }
    }
}
