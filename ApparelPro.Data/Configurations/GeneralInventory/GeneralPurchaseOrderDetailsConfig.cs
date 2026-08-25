using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.GeneralInventory
{
    public class GeneralPurchaseOrderDetailsConfig : IEntityTypeConfiguration<GeneralPurchaseOrderDetails>
    {
        public void Configure(EntityTypeBuilder<GeneralPurchaseOrderDetails> entity)
        {
            entity.ToTable("GeneralPurchaseOrderDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.PoNumber).HasColumnType("varchar(6)").HasColumnName("PoNumber").IsRequired();
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.RefNo).HasColumnType("varchar(10)").HasColumnName("RefNo");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(12,2)").HasColumnName("OrderedQuantity").IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,4)").HasColumnName("Price").IsRequired();
            entity.Property(e => e.ExpectedDate).HasColumnType("date").HasColumnName("ExpectedDate");
            entity.Property(e => e.Balance).HasColumnType("decimal(12,2)").HasColumnName("Balance").IsRequired();

            entity.HasOne<GeneralPurchaseOrder>()
                .WithMany()
                .HasForeignKey(e => e.PoNumber)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
