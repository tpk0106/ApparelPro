using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CertificateOfOriginLineConfig : IEntityTypeConfiguration<CertificateOfOriginLine>
    {
        public void Configure(EntityTypeBuilder<CertificateOfOriginLine> entity)
        {
            entity.ToTable("CertificateOfOriginLines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.HasIndex(e => e.InvoiceNumber);

            entity.Property(e => e.ShippingMarks).HasColumnType("varchar(100)");
            entity.Property(e => e.PackageTypeQuantity).HasColumnType("varchar(60)");
            entity.Property(e => e.ItemName).HasColumnType("varchar(100)");
            entity.Property(e => e.HsCode).HasColumnType("varchar(15)");
            entity.Property(e => e.NettWeight).HasColumnType("decimal(12,2)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(12,2)");
        }
    }
}
