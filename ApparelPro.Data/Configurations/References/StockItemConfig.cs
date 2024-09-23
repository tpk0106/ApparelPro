using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class StockItemConfig : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> entity)
        {
            entity.HasKey(k => new { k.StockCode, k.ItemCode });
            entity.Property(p => p.StockCode)                       
              .IsRequired()
               .HasColumnType("int");

            entity.Property(p => p.ItemCode)
                .UseIdentityColumn()
              .IsRequired()
              .HasColumnType("int");


            entity.Property(p => p.Description)
             .HasMaxLength(20)
             .IsRequired()
             .HasColumnType("nvarchar");
        }
    }
}
