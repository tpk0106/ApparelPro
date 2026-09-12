using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CommodityCodeConfig : IEntityTypeConfiguration<CommodityCode>
    {
        public void Configure(EntityTypeBuilder<CommodityCode> entity)
        {
            entity.ToTable("CommodityCodes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(8)");
            entity.Property(e => e.Description).HasColumnType("varchar(30)").IsRequired();
        }
    }
}
