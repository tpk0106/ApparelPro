using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class StyleMaterialCostProfileConfig:IEntityTypeConfiguration<StyleMaterialCostProfile>
    {
        public void Configure(EntityTypeBuilder<StyleMaterialCostProfile> entity)
        {
            entity.ToTable("StyleMaterialCostProfiles");

            entity.HasKey(e => new {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.StockCode,
                e.ItemCode,
                e.Feature1,
                e.Feature2,
                e.Feature3,
                e.Feature4
            });

            entity.Property(e => e.BuyerCode).HasColumnType("int").HasColumnName("Buyer");
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order");
            entity.Property(e => e.TypeCode).HasColumnType("int").HasColumnName("Type");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)").HasColumnName("Style");
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode");
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode");
            entity.Property(e => e.Feature1).HasColumnType("varchar(4)").HasColumnName("Feature1");
            entity.Property(e => e.Feature2).HasColumnType("varchar(4)").HasColumnName("Feature2");
            entity.Property(e => e.Feature3).HasColumnType("varchar(4)").HasColumnName("Feature3");
            entity.Property(e => e.Feature4).HasColumnType("varchar(4)").HasColumnName("Feature4");

            entity.Property(e => e.Description).HasColumnType("varchar(40)").HasColumnName("Description");
            entity.Property(e => e.ItemUnit).HasColumnType("varchar(3)").HasColumnName("ItemUnit");
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("CurrencyCode");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,4)").HasColumnName("Price");
            entity.Property(e => e.BalanceQuantity).HasColumnType("decimal(12,2)").HasColumnName("BalanceQuantity");

            // Legacy od_sacc2 fields (SUPP_CD / TOT_CON) added alongside the
            // entity properties. varchar(6) matches the same SupplierCode
            // convention used on PurchaseOrderHeaderConfig / StyleMaterialConsumptionLedgerConfig;
            // decimal(12,2) matches BalanceQuantity, since TotalConsumption tracks the
            // same kind of rolling planned-quantity figure.
            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode");
            entity.Property(e => e.TotalConsumption).HasColumnType("decimal(12,2)").HasColumnName("TotalConsumption");
        }
    }
}
