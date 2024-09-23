using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class StockConfig : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> entity)
        {
            entity.HasKey(k => k.Id);
            entity.Property(p => p.Description)
               .HasMaxLength(30)
               .IsRequired()
               .HasColumnType("nvarchar");
        }
    }
}
