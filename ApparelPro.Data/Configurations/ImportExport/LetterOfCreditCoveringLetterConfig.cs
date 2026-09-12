using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class LetterOfCreditCoveringLetterConfig : IEntityTypeConfiguration<LetterOfCreditCoveringLetter>
    {
        public void Configure(EntityTypeBuilder<LetterOfCreditCoveringLetter> entity)
        {
            entity.ToTable("LetterOfCreditCoveringLetters");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BankCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.LcNo).HasColumnType("varchar(20)").IsRequired();
            entity.HasIndex(e => new { e.BankCode, e.LcNo }).IsUnique();

            entity.Property(e => e.ExportLcNo).HasColumnType("varchar(20)");
            entity.Property(e => e.Value).HasColumnType("varchar(15)");
            entity.Property(e => e.Item1).HasColumnType("varchar(15)");
            entity.Property(e => e.Item2).HasColumnType("varchar(15)");
            entity.Property(e => e.Attn1).HasColumnType("varchar(20)");
            entity.Property(e => e.Attn2).HasColumnType("varchar(20)");
            entity.Property(e => e.SampleLcNo).HasColumnType("varchar(20)");
            entity.Property(e => e.Percentage).HasColumnType("decimal(5,1)");
            entity.Property(e => e.Box8Line1).HasColumnType("varchar(60)");
            entity.Property(e => e.Box8Line2).HasColumnType("varchar(60)");
            entity.Property(e => e.Box9Line1).HasColumnType("varchar(60)");
            entity.Property(e => e.Box9Line2).HasColumnType("varchar(60)");
            entity.Property(e => e.Box10Line1).HasColumnType("varchar(60)");
            entity.Property(e => e.Box10Line2).HasColumnType("varchar(60)");
        }
    }
}
