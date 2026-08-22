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

            // Tier 1 relationships audit (2026-08-16): these 2 columns were previously enforced
            // only by matching values, with no real database constraint.
            entity.HasOne<Stock>()
                .WithMany()
                .HasForeignKey(e => e.StockCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
