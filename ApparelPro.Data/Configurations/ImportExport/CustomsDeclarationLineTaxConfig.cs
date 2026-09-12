using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CustomsDeclarationLineTaxConfig : IEntityTypeConfiguration<CustomsDeclarationLineTax>
    {
        public void Configure(EntityTypeBuilder<CustomsDeclarationLineTax> entity)
        {
            entity.ToTable("CustomsDeclarationLineTaxes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CusNo).HasColumnType("varchar(15)").IsRequired();
            entity.Property(e => e.Item).HasColumnType("varchar(6)").IsRequired();
            entity.HasIndex(e => new { e.CusNo, e.Item });

            entity.Property(e => e.TaxCode).HasColumnType("varchar(4)");
            entity.Property(e => e.BaseCode).HasColumnType("varchar(3)");
            entity.Property(e => e.Rate).HasColumnType("decimal(6,1)");
            entity.Property(e => e.Amount).HasColumnType("decimal(11,2)");
            entity.Property(e => e.Exempted).HasColumnType("decimal(11,2)");
            entity.Property(e => e.Payable).HasColumnType("decimal(11,2)");
        }
    }
}
