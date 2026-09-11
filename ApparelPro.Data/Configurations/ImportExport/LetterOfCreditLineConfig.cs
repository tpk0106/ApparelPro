using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class LetterOfCreditLineConfig : IEntityTypeConfiguration<LetterOfCreditLine>
    {
        public void Configure(EntityTypeBuilder<LetterOfCreditLine> entity)
        {
            entity.ToTable("LetterOfCreditLines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BankCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.LcNo).HasColumnType("varchar(20)").IsRequired();
            entity.HasIndex(e => new { e.BankCode, e.LcNo });

            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)");
            entity.Property(e => e.Description).HasColumnType("varchar(60)");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Currency).HasColumnType("varchar(3)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(8,2)");
            entity.Property(e => e.BtnNo).HasColumnType("varchar(10)");
        }
    }
}
