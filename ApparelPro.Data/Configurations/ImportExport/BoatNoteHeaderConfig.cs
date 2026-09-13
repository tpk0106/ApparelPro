using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class BoatNoteHeaderConfig : IEntityTypeConfiguration<BoatNoteHeader>
    {
        public void Configure(EntityTypeBuilder<BoatNoteHeader> entity)
        {
            entity.ToTable("BoatNoteHeaders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();

            entity.Property(e => e.BoatNoteNumber).HasColumnType("varchar(30)");
            entity.Property(e => e.CustomsRegNo).HasColumnType("varchar(30)");
            entity.Property(e => e.CusDecRef).HasColumnType("varchar(30)");
            entity.Property(e => e.VesselName).HasColumnType("varchar(60)");
            entity.Property(e => e.VoyageNo).HasColumnType("varchar(20)");
            entity.Property(e => e.PortOfLoadingCode).HasColumnType("varchar(3)");
            entity.Property(e => e.DischargePortCode).HasColumnType("varchar(3)");
            entity.Property(e => e.Remarks).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomsOfficerStatus).HasColumnType("varchar(40)");
            entity.Property(e => e.CustomsOfficerReference).HasColumnType("varchar(40)");
            entity.Property(e => e.TerminalOperatorReleaseStatus).HasColumnType("varchar(40)");
            entity.Property(e => e.TerminalOperatorReference).HasColumnType("varchar(40)");
            entity.Property(e => e.ShipperAgentSignOffStatus).HasColumnType("varchar(40)");
            entity.Property(e => e.ChaLicenseNo).HasColumnType("varchar(30)");

            entity.HasOne<CompanyAddress>()
                .WithMany()
                .HasForeignKey(e => e.CompanyAddressId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
