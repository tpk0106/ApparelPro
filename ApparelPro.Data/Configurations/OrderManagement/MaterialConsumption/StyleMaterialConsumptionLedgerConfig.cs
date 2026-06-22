using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class StyleMaterialConsumptionLedgerConfig:IEntityTypeConfiguration<StyleMaterialConsumptionLedger>
    {
        public void Configure(EntityTypeBuilder<StyleMaterialConsumptionLedger> entity)
        {
            entity.ToTable("StyleMaterialConsumptionLedger");

            entity.HasKey(e => new {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.Color,
                e.Size,
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
            entity.Property(e => e.Color).HasColumnType("varchar(6)").HasColumnName("Color");
            entity.Property(e => e.Size).HasColumnType("varchar(10)").HasColumnName("Size");
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode");
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode");
            entity.Property(e => e.Feature1).HasColumnType("varchar(4)").HasColumnName("Feature1");
            entity.Property(e => e.Feature2).HasColumnType("varchar(4)").HasColumnName("Feature2");
            entity.Property(e => e.Feature3).HasColumnType("varchar(4)").HasColumnName("Feature3");
            entity.Property(e => e.Feature4).HasColumnType("varchar(4)").HasColumnName("Feature4");

            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode");
            entity.Property(e => e.ConsumptionUnit).HasColumnType("varchar(3)").HasColumnName("ConsumptionUnit");
            entity.Property(e => e.ItemUnit).HasColumnType("varchar(3)").HasColumnName("ItemUnit");
            entity.Property(e => e.QuantityPerGarment).HasColumnType("decimal(8,3)").HasColumnName("QuantityPerGarment");
            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode");
            entity.Property(e => e.TotalConsumption).HasColumnType("decimal(12,2)").HasColumnName("TotalConsumption");
            entity.Property(e => e.PercentageAllowance).HasColumnType("decimal(4,1)").HasColumnName("PercentageAllowance");

            entity.Property(e => e.IsAdditionalCost).HasColumnType("bit").HasColumnName("IsAdditionalCost");
            entity.Property(e => e.CalculateConsumption).HasColumnType("bit").HasColumnName("CalculateConsumption");
        }
    }
}
