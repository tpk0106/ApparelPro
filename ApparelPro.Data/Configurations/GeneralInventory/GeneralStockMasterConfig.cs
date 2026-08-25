using ApparelPro.Data.Models.GeneralInventory;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.GeneralInventory
{
    public class GeneralStockMasterConfig : IEntityTypeConfiguration<GeneralStockMaster>
    {
        public void Configure(EntityTypeBuilder<GeneralStockMaster> entity)
        {
            entity.ToTable("GeneralStockMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();

            entity.Property(e => e.QtyInHand).HasColumnType("decimal(12,2)").HasColumnName("QtyInHand").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.ShadowBalance).HasColumnType("decimal(12,2)").HasColumnName("ShadowBalance").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.Value).HasColumnType("decimal(12,2)").HasColumnName("Value").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency").IsRequired();
            entity.Property(e => e.DamagedQuantity).HasColumnType("decimal(12,2)").HasColumnName("DamagedQuantity").IsRequired().HasDefaultValue(0m);

            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(12,2)").HasColumnName("ReorderLevel").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.ReorderQuantity).HasColumnType("decimal(12,2)").HasColumnName("ReorderQuantity").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.MinStock).HasColumnType("decimal(12,2)").HasColumnName("MinStock").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.MaxStock).HasColumnType("decimal(12,2)").HasColumnName("MaxStock").IsRequired().HasDefaultValue(0m);

            entity.HasIndex(e => new { e.StoreCode, e.ItemCode }).IsUnique();

            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(e => e.Currency)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
