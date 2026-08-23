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
                e.ItemCode
            });

            entity.Property(e => e.BuyerCode).HasColumnType("int").HasColumnName("Buyer");
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order");
            entity.Property(e => e.TypeCode).HasColumnType("int").HasColumnName("Type");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)").HasColumnName("Style");

            // Collapsed 2026-07-22 from separate StockCode(2)/ItemCode(4)/Feature1-4(4 each)
            // columns into the single 22-char composite already used by OrderwiseStockMaster,
            // OrderwiseStockTransaction and SupplierPurchaseOrderDetails.ItemCode.
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode");

            entity.Property(e => e.Description).HasColumnType("varchar(100)").HasColumnName("Description");
            entity.Property(e => e.ItemUnit).HasColumnType("varchar(3)").HasColumnName("ItemUnit");
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("CurrencyCode");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,4)").HasColumnName("Price");
            entity.Property(e => e.BalanceQuantity).HasColumnType("decimal(12,2)").HasColumnName("BalanceQuantity");

            // Legacy od_sacc2 fields (SUPP_CD / TOT_CON) added alongside the
            // entity properties. varchar(6) matches the same SupplierCode
            // convention used on SupplierPurchaseOrderConfig / StyleMaterialConsumptionLedgerConfig;
            // decimal(12,2) matches BalanceQuantity, since TotalConsumption tracks the
            // same kind of rolling planned-quantity figure.
            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode");
            entity.Property(e => e.TotalConsumption).HasColumnType("decimal(12,2)").HasColumnName("TotalConsumption");
        }
    }
}
