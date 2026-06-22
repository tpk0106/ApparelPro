using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class OrderwiseStockMasterConfig : IEntityTypeConfiguration<OrderwiseStockMaster>
    {
        public void Configure(EntityTypeBuilder<OrderwiseStockMaster> entity)
        {
            entity.ToTable("OrderwiseStockMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.BuyerCode).HasColumnName("BuyerCode").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(40)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency").IsRequired();
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(12,2)").HasColumnName("OrderedQuantity").IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,4)").HasColumnName("Price").IsRequired();
        }
    }
}
