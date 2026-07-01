using ApparelPro.Data.Models.OrderwiseInventory;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class OrderwiseStockConfig : IEntityTypeConfiguration<OrderwiseStock>
    {
        public void Configure(EntityTypeBuilder<OrderwiseStock> entity)
        {
            entity.ToTable("OrderwiseStocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.BuyerCode).HasColumnName("BuyerCode").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order").IsRequired();
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(40)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();

            // Precision configurations for numeric inventory values
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(12,2)").HasColumnName("OrderedQuantity").IsRequired();
            entity.Property(e => e.QtyInHand).HasColumnName("QtyInHand").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ShadowBalance).HasColumnName("ShadowBalance").HasColumnType("decimal(12,2)");
            entity.Property(e => e.DamagedQuantity).HasColumnName("DamagedQuantity").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ToDateIssued).HasColumnName("ToDateIssued").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ToDateReceived).HasColumnName("ToDateReceived").HasColumnType("decimal(12,2)");
            entity.Property(e => e.SrnBalance).HasColumnName("SrnBalance").HasColumnType("decimal(12,2)");

            entity.Property(e => e.LastDateIssued).HasColumnName("LastDateIssued").HasColumnType("date");
            entity.Property(e => e.LastDateReceived).HasColumnName("LastDateReceived").HasColumnType("date");

        }
    }
}
