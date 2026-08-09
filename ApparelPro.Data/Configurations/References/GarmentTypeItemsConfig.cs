using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class GarmentTypeItemsConfig : IEntityTypeConfiguration<GarmentTypeItems>
    {
        public void Configure(EntityTypeBuilder<GarmentTypeItems> entity)
        {
            entity.ToTable("GarmentTypeItems");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.GarmentTypeId).HasColumnName("GarmentTypeId").IsRequired();
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").HasColumnName("StockCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(6)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(8,3)").HasColumnName("Quantity").IsRequired();

            entity.HasOne(e => e.GarmentType)
                  .WithMany()
                  .HasForeignKey(e => e.GarmentTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
