using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CustomsProcedureCodeConfig : IEntityTypeConfiguration<CustomsProcedureCode>
    {
        public void Configure(EntityTypeBuilder<CustomsProcedureCode> entity)
        {
            entity.ToTable("CustomsProcedureCodes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(4)");
            entity.Property(e => e.Description).HasColumnType("varchar(40)").IsRequired();
        }
    }
}
