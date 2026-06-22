using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class StockConfig : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> entity)
        {
            entity.ToTable("Stocks");

            // 1. FIXED PRIMARY KEY: Enforce StockCode string as the master lookup identifier
            entity.HasKey(k => k.StockCode);

            entity.Property(e => e.StockCode)
                .HasColumnType("varchar(2)") // Restricted to exactly 2 characters matching Clipper limits
                .HasColumnName("StockCode")
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnType("varchar(30)") // Switched to optimal varchar to clean up allocation padding
                .HasColumnName("Description")
                .IsRequired();
        }
    }
}
