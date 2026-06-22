using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class OrderItemFeatureConfig:IEntityTypeConfiguration<OrderItemFeature>
    {
        public void Configure(EntityTypeBuilder<OrderItemFeature> entity)
        {
            entity.ToTable("OrderItemFeatures");
            entity.HasKey(e => new { e.StockCode, e.ItemCode });

            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode");
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode");

            entity.Property(e => e.Feature1Type).HasColumnType("varchar(4)").HasColumnName("Feature1");
            entity.Property(e => e.Feature2Type).HasColumnType("varchar(4)").HasColumnName("Feature2");
            entity.Property(e => e.Feature3Type).HasColumnType("varchar(4)").HasColumnName("Feature3");
            entity.Property(e => e.Feature4Type).HasColumnType("varchar(4)").HasColumnName("Feature4");

            entity.Property(e => e.CostPerUnit).HasColumnType("decimal(10,4)").HasColumnName("CostPerUnit");
        }
    }
}
