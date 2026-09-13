using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class BoatNoteCargoLineConfig : IEntityTypeConfiguration<BoatNoteCargoLine>
    {
        public void Configure(EntityTypeBuilder<BoatNoteCargoLine> entity)
        {
            entity.ToTable("BoatNoteCargoLines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.HasIndex(e => e.InvoiceNumber);

            entity.Property(e => e.ContainerNo).HasColumnType("varchar(20)").IsRequired();
            entity.Property(e => e.SealNo).HasColumnType("varchar(20)");
            entity.Property(e => e.PackageQuantity).HasColumnType("varchar(30)");
            entity.Property(e => e.Description).HasColumnType("varchar(100)");
            entity.Property(e => e.HsCode).HasColumnType("varchar(15)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(12,2)");
            entity.Property(e => e.WeightUnit).HasColumnType("varchar(6)");
        }
    }
}
