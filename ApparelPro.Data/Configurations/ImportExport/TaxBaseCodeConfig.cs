using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class TaxBaseCodeConfig : IEntityTypeConfiguration<TaxBaseCode>
    {
        public void Configure(EntityTypeBuilder<TaxBaseCode> entity)
        {
            entity.ToTable("TaxBaseCodes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(3)");
            entity.Property(e => e.Description).HasColumnType("varchar(30)").IsRequired();
        }
    }
}
