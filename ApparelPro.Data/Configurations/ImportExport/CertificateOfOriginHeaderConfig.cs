using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CertificateOfOriginHeaderConfig : IEntityTypeConfiguration<CertificateOfOriginHeader>
    {
        public void Configure(EntityTypeBuilder<CertificateOfOriginHeader> entity)
        {
            entity.ToTable("CertificateOfOriginHeaders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.Property(e => e.RefNo).HasColumnType("varchar(20)");
            entity.Property(e => e.CountryOfOrigin).HasColumnType("varchar(3)");
            entity.Property(e => e.PortOfLoading).HasColumnType("varchar(40)");
            entity.Property(e => e.OtherRemarks).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CompetentAuthorityName).HasColumnType("varchar(40)");
            entity.Property(e => e.IssuePlace).HasColumnType("varchar(40)");
            entity.Property(e => e.RequestSubmittedBy).HasColumnType("varchar(40)");

            entity.HasOne<CompanyAddress>()
                .WithMany()
                .HasForeignKey(e => e.CompanyAddressId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
