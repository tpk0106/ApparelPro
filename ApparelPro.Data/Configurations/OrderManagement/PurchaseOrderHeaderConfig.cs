using ApparelPro.Data.Models.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class PurchaseOrderHeaderConfig : IEntityTypeConfiguration<PurchaseOrderHeader>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderHeader> entity)
        {
            entity.ToTable("PurchaseOrderHeaders");

            // Set up string primary key based on your Purchase Order number format
            entity.HasKey(e => e.PurchaseOrderNumber);
            entity.Property(e => e.PurchaseOrderNumber).HasColumnType("varchar(10)").HasColumnName("PurchaseOrderNumber").IsRequired();

            entity.Property(e => e.CreatedDate).HasColumnType("date").HasColumnName("CreatedDate");
            entity.Property(e => e.CreatedTime).HasColumnType("time").HasColumnName("CreatedTime");

            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode").IsRequired();
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode").IsRequired();
            entity.Property(e => e.ProformaInvoiceNo).HasColumnType("varchar(20)").HasColumnName("ProformaInvoiceNo");
            entity.Property(e => e.ProformaInvoiceDate).HasColumnType("date").HasColumnName("ProformaInvoiceDate");
            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)").HasColumnName("CurrencyCode").IsRequired();

            // Concurrency tracking bit column
            entity.Property(e => e.IsPoUsed).HasColumnType("bit").HasColumnName("IsPoUsed").IsRequired();
        }
    }
}
