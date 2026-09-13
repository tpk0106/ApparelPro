using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class ValueDeclarationHeaderConfig : IEntityTypeConfiguration<ValueDeclarationHeader>
    {
        public void Configure(EntityTypeBuilder<ValueDeclarationHeader> entity)
        {
            entity.ToTable("ValueDeclarationHeaders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)");
            entity.HasIndex(e => e.InvoiceNumber);

            entity.Property(e => e.Year).HasColumnType("varchar(4)");
            entity.Property(e => e.OfficeCode).HasColumnType("varchar(10)");
            entity.Property(e => e.SeriesLetter).HasColumnType("varchar(5)");
            entity.Property(e => e.CusDecNo).HasColumnType("varchar(20)");
            entity.Property(e => e.PreviousImportsDetails).HasColumnType("varchar(200)");

            entity.Property(e => e.ExporterName).HasColumnType("varchar(100)").IsRequired();
            entity.Property(e => e.ExporterAddress).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IndentingAgentName).HasColumnType("varchar(100)");
            entity.Property(e => e.IndentingAgentAddress).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ImporterVatNo).HasColumnType("varchar(20)");
            entity.Property(e => e.DeclarantVatNo).HasColumnType("varchar(20)");
            entity.Property(e => e.SalesContractNo).HasColumnType("varchar(30)");
            entity.Property(e => e.InvoiceNo).HasColumnType("varchar(25)").IsRequired();
            entity.HasIndex(e => e.InvoiceNo);

            entity.Property(e => e.TotalInvoiceValue).HasColumnType("decimal(14,2)");
            entity.Property(e => e.NatureOfTransaction).HasColumnType("varchar(60)");
            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)");
            entity.Property(e => e.TermsOfDeliveryCode).HasColumnType("varchar(3)");

            entity.Property(e => e.BrokerageCommission).HasColumnType("decimal(14,2)");
            entity.Property(e => e.CostOfContainers).HasColumnType("decimal(14,2)");
            entity.Property(e => e.PackingCosts).HasColumnType("decimal(14,2)");
            entity.Property(e => e.CostOfGoodsSuppliedByBuyer).HasColumnType("decimal(14,2)");
            entity.Property(e => e.RoyaltiesLicenseFees).HasColumnType("decimal(14,2)");
            entity.Property(e => e.ProceedsToSeller).HasColumnType("decimal(14,2)");
            entity.Property(e => e.LoadingHandlingCharges).HasColumnType("decimal(14,2)");
            entity.Property(e => e.Insurance).HasColumnType("decimal(14,2)");
            entity.Property(e => e.Freight).HasColumnType("decimal(14,2)");
            entity.Property(e => e.OtherPayments).HasColumnType("decimal(14,2)");

            entity.Property(e => e.TermsOfPaymentCode).HasColumnType("varchar(3)");
            entity.Property(e => e.PortOfShipmentCode).HasColumnType("varchar(3)");
            entity.Property(e => e.AwbBlNo).HasColumnType("varchar(20)");

            entity.Property(e => e.SignatoryName).HasColumnType("varchar(40)");
            entity.Property(e => e.SignatoryTitle).HasColumnType("varchar(40)");
            entity.Property(e => e.SignatoryCompanyName).HasColumnType("varchar(100)");

            entity.Property(e => e.AppraiserComments).HasColumnType("varchar(200)");
            entity.Property(e => e.ScComments).HasColumnType("varchar(200)");
            entity.Property(e => e.ValuationReferenceNo).HasColumnType("varchar(30)");
            entity.Property(e => e.CentralValuationEndorsement).HasColumnType("varchar(200)");

            entity.HasOne<CompanyAddress>()
                .WithMany()
                .HasForeignKey(e => e.ImporterCompanyAddressId)
                .HasConstraintName("FK_ValueDeclarationHeaders_CompanyAddresses_ImporterCompanyAddressId")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<CompanyAddress>()
                .WithMany()
                .HasForeignKey(e => e.CompanyAddressId)
                .HasConstraintName("FK_ValueDeclarationHeaders_CompanyAddresses_CompanyAddressId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
