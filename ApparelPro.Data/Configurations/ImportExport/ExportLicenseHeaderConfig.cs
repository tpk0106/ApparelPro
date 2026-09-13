using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class ExportLicenseHeaderConfig : IEntityTypeConfiguration<ExportLicenseHeader>
    {
        public void Configure(EntityTypeBuilder<ExportLicenseHeader> entity)
        {
            entity.ToTable("ExportLicenseHeaders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicantType).HasColumnType("varchar(30)");
            entity.Property(e => e.BusinessRegistrationNo).HasColumnType("varchar(30)");
            entity.Property(e => e.VatRegistrationNo).HasColumnType("varchar(20)");
            entity.Property(e => e.Telephone).HasColumnType("varchar(20)");
            entity.Property(e => e.Fax).HasColumnType("varchar(20)");
            entity.Property(e => e.Email).HasColumnType("varchar(60)");
            entity.Property(e => e.ApplicantIdOfficeUse).HasColumnType("varchar(30)");

            entity.Property(e => e.LicenseType).HasColumnType("varchar(20)");
            entity.Property(e => e.ExchangeType).HasColumnType("varchar(20)");
            entity.Property(e => e.CommercialType).HasColumnType("varchar(20)");
            entity.Property(e => e.BankCode).HasColumnType("varchar(10)");
            entity.Property(e => e.ModeOfPayment).HasColumnType("varchar(10)");
            entity.Property(e => e.ModeOfTransportation).HasColumnType("varchar(20)");

            entity.Property(e => e.PurposeOfExportation).HasColumnType("varchar(200)");
            entity.Property(e => e.UseOfCommodity).HasColumnType("varchar(200)");

            entity.HasOne<CompanyAddress>()
                .WithMany()
                .HasForeignKey(e => e.CompanyAddressId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
