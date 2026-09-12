using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CustomsDeclarationLineConfig : IEntityTypeConfiguration<CustomsDeclarationLine>
    {
        public void Configure(EntityTypeBuilder<CustomsDeclarationLine> entity)
        {
            entity.ToTable("CustomsDeclarationLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CusNo).HasColumnType("varchar(15)").IsRequired();
            entity.HasIndex(e => e.CusNo);
            entity.Property(e => e.Item).HasColumnType("varchar(6)").IsRequired();

            entity.Property(e => e.CustomsProcedureCode).HasColumnType("varchar(4)");
            entity.Property(e => e.CommodityCode).HasColumnType("varchar(8)");
            entity.Property(e => e.NetWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.GrossWeight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.SupplementaryUnitCode).HasColumnType("varchar(3)");
            entity.Property(e => e.SupplementaryQty).HasColumnType("decimal(11,2)");
            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)");
            entity.Property(e => e.Fob).HasColumnType("decimal(11,2)");
            entity.Property(e => e.Freight).HasColumnType("decimal(11,2)");
            entity.Property(e => e.Insurance).HasColumnType("decimal(11,2)");
            entity.Property(e => e.Other).HasColumnType("decimal(11,2)");
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(12,4)");
            entity.Property(e => e.CountryCode).HasColumnType("varchar(2)");
            entity.Property(e => e.LicenceNo).HasColumnType("varchar(15)");
            entity.Property(e => e.AgreementCode).HasColumnType("varchar(2)");
            entity.Property(e => e.QtyDeducted).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Value).HasColumnType("varchar(15)");
            entity.Property(e => e.AnyOther).HasColumnType("varchar(15)");
            entity.Property(e => e.Detail).HasColumnType("nvarchar(max)");
        }
    }
}
