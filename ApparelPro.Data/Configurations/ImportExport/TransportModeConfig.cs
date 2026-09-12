using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class TransportModeConfig : IEntityTypeConfiguration<TransportMode>
    {
        public void Configure(EntityTypeBuilder<TransportMode> entity)
        {
            entity.ToTable("TransportModes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(2)");
            entity.Property(e => e.Description).HasColumnType("varchar(30)").IsRequired();
        }
    }
}
