using ApparelPro.Data.Models.OrderwiseInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class GeneralStockReferenceConfig : IEntityTypeConfiguration<GeneralStockReference>
    {
        public void Configure(EntityTypeBuilder<GeneralStockReference> entity)
        {
            entity.ToTable("GeneralStockReferences");
            entity.HasKey(e => e.ItemCode);

            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("nvarchar(60)");
        }
    }
}
