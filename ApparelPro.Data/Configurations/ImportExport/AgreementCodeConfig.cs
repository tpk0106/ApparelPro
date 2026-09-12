using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class AgreementCodeConfig : IEntityTypeConfiguration<AgreementCode>
    {
        public void Configure(EntityTypeBuilder<AgreementCode> entity)
        {
            entity.ToTable("AgreementCodes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(2)");
            entity.Property(e => e.Description).HasColumnType("varchar(40)").IsRequired();
        }
    }
}
