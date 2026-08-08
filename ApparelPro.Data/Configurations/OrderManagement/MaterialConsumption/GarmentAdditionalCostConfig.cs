using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class GarmentAdditionalCostConfig : IEntityTypeConfiguration<GarmentAdditionalCost>
    {
        public void Configure(EntityTypeBuilder<GarmentAdditionalCost> entity)
        {
            entity.ToTable("GarmentAdditionalCosts");

            entity.HasKey(e => new {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.AdditionalCostCode,
                e.ItemCode
            });

            entity.Property(e => e.BuyerCode).HasColumnType("int").HasColumnName("Buyer");
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order");
            entity.Property(e => e.TypeCode).HasColumnType("int").HasColumnName("Type");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)").HasColumnName("Style");
            entity.Property(e => e.AdditionalCostCode).HasColumnType("varchar(3)").HasColumnName("AdditionalCostCode");
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode");

            entity.Property(e => e.Color).HasColumnType("varchar(6)").HasColumnName("Color");
            entity.Property(e => e.Size).HasColumnType("varchar(10)").HasColumnName("Size");
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode");
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit");
            entity.Property(e => e.Quantity).HasColumnType("decimal(8,3)").HasColumnName("Quantity");
            entity.Property(e => e.Cost).HasColumnType("decimal(12,4)").HasColumnName("Cost");
            entity.Property(e => e.IsCostPerGarment).HasColumnType("bit").HasColumnName("IsCostPerGarment");
            entity.Property(e => e.IsSemiFinishedGarment).HasColumnType("bit").HasColumnName("IsSemiFinishedGarment");
        }
    }
}
