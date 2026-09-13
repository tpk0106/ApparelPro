using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class ExportLicenseLineConfig : IEntityTypeConfiguration<ExportLicenseLine>
    {
        public void Configure(EntityTypeBuilder<ExportLicenseLine> entity)
        {
            entity.ToTable("ExportLicenseLines");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ExportLicenseHeaderId);

            entity.Property(e => e.HsNumber).HasColumnType("varchar(20)");
            entity.Property(e => e.Description).HasColumnType("varchar(150)").IsRequired();
            entity.Property(e => e.PackSize).HasColumnType("varchar(30)");
            entity.Property(e => e.UnitCode).HasColumnType("varchar(6)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(14,2)");
            entity.Property(e => e.Insurance).HasColumnType("decimal(14,2)");
            entity.Property(e => e.Freight).HasColumnType("decimal(14,2)");
            entity.Property(e => e.TotalCif).HasColumnType("decimal(14,2)");

            entity.HasOne<ExportLicenseHeader>()
                .WithMany()
                .HasForeignKey(e => e.ExportLicenseHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
