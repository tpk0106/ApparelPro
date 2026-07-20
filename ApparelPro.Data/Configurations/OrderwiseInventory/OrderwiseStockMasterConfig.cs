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
            // Order is always 12 chars (verified against IN_STMST.DBF: ORDER C(12))
            entity.Property(e => e.Order).HasColumnType("varchar(12)").HasColumnName("Order").IsRequired();
            // ItemCode is the full 22-char composite key: StockCode(2)+ItemCode(4)+Feature1-4(4 each) = 22
            // (verified against IN_STMST.DBF: ITEM_CD C(22))
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency").IsRequired();
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(12,2)").HasColumnName("OrderedQuantity").IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,4)").HasColumnName("Price").IsRequired();
            entity.Property(e => e.RequisitionedQuantity).HasColumnName("RequisitionedQuantity").HasColumnType("decimal(12,2)");
            entity.Property(e => e.IssuedQuantity).HasColumnType("decimal(12,2)").HasColumnName("IssuedQuantity").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(12,2)").HasColumnName("ReceivedQuantity").IsRequired().HasDefaultValue(0m);
        }
    }
}
