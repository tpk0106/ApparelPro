using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CustomsDeclarationHeaderConfig : IEntityTypeConfiguration<CustomsDeclarationHeader>
    {
        public void Configure(EntityTypeBuilder<CustomsDeclarationHeader> entity)
        {
            entity.ToTable("CustomsDeclarationHeaders");
            entity.HasKey(e => e.CusNo);
            entity.Property(e => e.CusNo).HasColumnType("varchar(15)");

            entity.Property(e => e.ExporterCode).HasColumnType("varchar(11)");
            entity.Property(e => e.BoiRegNo).HasColumnType("varchar(15)");
            entity.Property(e => e.ConsigneeCode).HasColumnType("varchar(11)");
            entity.Property(e => e.NotifyPartyCode).HasColumnType("varchar(11)");
            entity.Property(e => e.DeclarantCode).HasColumnType("varchar(11)");
            entity.Property(e => e.ClearanceOfficeCode).HasColumnType("varchar(4)");
            entity.Property(e => e.FrontierOfficeCode).HasColumnType("varchar(4)");
            entity.Property(e => e.CountryOfConsignmentCode).HasColumnType("varchar(2)");
            entity.Property(e => e.LocationOfGoods).HasColumnType("varchar(10)");
            entity.Property(e => e.CountryOfOriginCode).HasColumnType("varchar(2)");
            entity.Property(e => e.CountryOfDestinationCode).HasColumnType("varchar(2)");
            entity.Property(e => e.WarehouseNo).HasColumnType("varchar(10)");
            entity.Property(e => e.WarehousePeriod).HasColumnType("varchar(10)");
            entity.Property(e => e.PrecedingDocNo).HasColumnType("varchar(10)");
            entity.Property(e => e.VoyageNo).HasColumnType("varchar(10)");
            entity.Property(e => e.BlAwbNo).HasColumnType("varchar(15)");
            entity.Property(e => e.PaymentTermCode).HasColumnType("varchar(3)");
            entity.Property(e => e.DeliveryTermCode).HasColumnType("varchar(3)");
            entity.Property(e => e.Vessel).HasColumnType("varchar(20)");
            entity.Property(e => e.PortOfLoadingCode).HasColumnType("varchar(2)");
            entity.Property(e => e.TransportModeCode).HasColumnType("varchar(2)");
            entity.Property(e => e.PrepaymentAccountName).HasColumnType("varchar(20)");
            entity.Property(e => e.PrepaymentAccountNo).HasColumnType("varchar(15)");
            entity.Property(e => e.PortOfDischargeCode).HasColumnType("varchar(2)");
            entity.Property(e => e.PlaceOfDeliveryCode).HasColumnType("varchar(2)");
            entity.Property(e => e.BankCode).HasColumnType("varchar(8)");
            entity.Property(e => e.ReferenceNo).HasColumnType("varchar(10)");
            entity.Property(e => e.Remark1).HasColumnType("varchar(60)");
            entity.Property(e => e.Remark2).HasColumnType("varchar(60)");
            entity.Property(e => e.Remark3).HasColumnType("varchar(60)");
            entity.Property(e => e.Remark4).HasColumnType("varchar(60)");
            entity.Property(e => e.DeclarantName).HasColumnType("varchar(30)");
            entity.Property(e => e.SubmittedByName).HasColumnType("varchar(30)");
        }
    }
}
