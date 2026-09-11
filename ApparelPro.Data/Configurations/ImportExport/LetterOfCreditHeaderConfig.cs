using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class LetterOfCreditHeaderConfig : IEntityTypeConfiguration<LetterOfCreditHeader>
    {
        public void Configure(EntityTypeBuilder<LetterOfCreditHeader> entity)
        {
            entity.ToTable("LetterOfCreditHeaders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BankCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.LcNo).HasColumnType("varchar(20)").IsRequired();
            entity.HasIndex(e => new { e.BankCode, e.LcNo }).IsUnique();

            entity.Property(e => e.CreditNo).HasColumnType("varchar(15)");
            entity.Property(e => e.ExpiryPlaceCode).HasColumnType("varchar(3)");
            entity.Property(e => e.IssuedBy).HasColumnType("varchar(1)");
            entity.Property(e => e.NotifyPartyCode).HasColumnType("varchar(6)");
            entity.Property(e => e.LicenceType).HasColumnType("varchar(6)");
            entity.Property(e => e.LicenceNo).HasColumnType("varchar(15)");
            entity.Property(e => e.TransferableCredit).HasColumnType("varchar(1)");
            entity.Property(e => e.ConfirmedCredit).HasColumnType("varchar(1)");
            entity.Property(e => e.PartShipment).HasColumnType("varchar(1)");
            entity.Property(e => e.Transhipment).HasColumnType("varchar(1)");
            entity.Property(e => e.InsuranceCoverage).HasColumnType("varchar(1)");
            entity.Property(e => e.ShipmentTerm).HasColumnType("varchar(1)");
            entity.Property(e => e.ShipmentTermOther).HasColumnType("varchar(10)");
            entity.Property(e => e.BillOfLadingIssued).HasColumnType("varchar(1)");
            entity.Property(e => e.FreightPayment).HasColumnType("varchar(1)");
            entity.Property(e => e.AirwayDocumentType).HasColumnType("varchar(1)");
            entity.Property(e => e.AdditionalConditions).HasColumnType("varchar(1)");
            entity.Property(e => e.ExtraConditions).HasColumnType("varchar(1)");
            entity.Property(e => e.CreditBy).HasColumnType("varchar(1)");
            entity.Property(e => e.BeneficiaryDraft).HasColumnType("varchar(1)");
            entity.Property(e => e.InsuranceClause).HasColumnType("varchar(1)");
            entity.Property(e => e.CountryOfOriginCode).HasColumnType("varchar(3)");
            entity.Property(e => e.ShipmentFromCode).HasColumnType("varchar(3)");
            entity.Property(e => e.TransportTo).HasColumnType("varchar(40)");
            entity.Property(e => e.InsurancePercent).HasColumnType("decimal(5,1)");
            entity.Property(e => e.InsuranceValueCurrency).HasColumnType("varchar(3)");
            entity.Property(e => e.CertifiedMailCopies).HasColumnType("varchar(3)");
            entity.Property(e => e.DocumentPresentationDays).HasColumnType("varchar(3)");
            entity.Property(e => e.ShipmentTermCustomLabel).HasColumnType("varchar(10)");
            entity.Property(e => e.AccountNo).HasColumnType("varchar(15)");
            entity.Property(e => e.Branch).HasColumnType("varchar(20)");
            entity.Property(e => e.CreditAvailableWith).HasColumnType("varchar(20)");
            entity.Property(e => e.CreditDocuments).HasColumnType("varchar(10)");
            entity.Property(e => e.ConformityWith).HasColumnType("varchar(50)");
            entity.Property(e => e.InsuranceRemarks).HasColumnType("varchar(50)");
            entity.Property(e => e.TenorDays).HasColumnType("varchar(3)");
            entity.Property(e => e.DrawnOn).HasColumnType("varchar(20)");
            entity.Property(e => e.CiCopies).HasColumnType("varchar(3)");
            entity.Property(e => e.BeneficiaryCountryCode).HasColumnType("varchar(3)");
            entity.Property(e => e.AdvisingBankCode).HasColumnType("varchar(3)");
            entity.Property(e => e.InvoiceSelection).HasColumnType("varchar(1)");
            entity.Property(e => e.PackingSpecification).HasColumnType("varchar(3)");
            entity.Property(e => e.MarineBillOfLading).HasColumnType("varchar(40)");
            entity.Property(e => e.MarineBillOfLadingConsignee).HasColumnType("varchar(40)");
            entity.Property(e => e.AirWaybill).HasColumnType("varchar(40)");
            entity.Property(e => e.AirWaybillConsignee).HasColumnType("varchar(40)");
            entity.Property(e => e.OtherDocuments).HasColumnType("varchar(40)");
            entity.Property(e => e.BankSentTo).HasColumnType("varchar(50)");
            entity.Property(e => e.ImportPermitNo).HasColumnType("varchar(20)");
            entity.Property(e => e.ImportContractNo).HasColumnType("varchar(15)");
            entity.Property(e => e.IncomeTaxNo).HasColumnType("varchar(15)");
            entity.Property(e => e.BttReferenceNo).HasColumnType("varchar(15)");
        }
    }
}
