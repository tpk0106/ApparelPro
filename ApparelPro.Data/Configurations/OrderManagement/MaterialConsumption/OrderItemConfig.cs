using ApparelPro.Data.Models.OrderManagement.MaterialConsumption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement.MaterialConsumption
{
    public class OrderItemConfig:IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> entity)
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => new { e.StockCode, e.ItemCode });

            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode");
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode");
            entity.Property(e => e.Description).HasColumnType("varchar(30)").HasColumnName("Description");
        }
    }
}
