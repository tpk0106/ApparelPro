using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class StockItemConfig : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> entity)
        {
            entity.ToTable("StockItems");

            // 1. PRIMARY COMPOSITE STRING KEY: Combines StockCode and ItemCode
            entity.HasKey(k => new { k.StockCode, k.ItemCode });

            entity.Property(e => e.StockCode)
                .HasColumnType("varchar(2)")
                .HasColumnName("StockCode")
                .IsRequired();

            entity.Property(e => e.ItemCode)
                .HasColumnType("varchar(4)") // Up to 6 characters matching your legacy parameters
                .HasColumnName("ItemCode")
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnType("varchar(100)")
                .HasColumnName("Description")
                .IsRequired();

            // 2. Setup the Foreign Key constraint pointing back to the updated string-keyed Stocks table
            entity.HasOne(e => e.Stock)
                  .WithMany()
                  .HasForeignKey(e => e.StockCode)
                  .OnDelete(DeleteBehavior.Restrict);
            //entity.HasKey(k => new { k.StockCode, k.ItemCode });
            //entity.Property(p => p.StockCode)                       
            //  .IsRequired()
            //   .HasColumnType("int");

            //entity.Property(p => p.ItemCode)
            //    .UseIdentityColumn()
            //  .IsRequired()
            //  .HasColumnType("int");


            //entity.Property(p => p.Description)
            // .HasMaxLength(20)
            // .IsRequired()
            // .HasColumnType("nvarchar");
        }
    }
}
