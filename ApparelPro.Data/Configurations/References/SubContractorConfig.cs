using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class SubContractorConfig : IEntityTypeConfiguration<SubContractor>
    {
        public void Configure(EntityTypeBuilder<SubContractor> entity)
        {
            entity.ToTable("SubContractors");
            entity.HasKey(e => e.Code);

            // Code is space(6) in legacy od_scref.dbf (m_subcnt = space(6) in IN_AIN1.PRG).
            entity.Property(e => e.Code).HasColumnType("varchar(6)").HasColumnName("Code");
            entity.Property(e => e.Name).IsRequired().HasColumnType("varchar(40)").HasColumnName("Name");
        }
    }
}
