using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class ValueDeclarationLineConfig : IEntityTypeConfiguration<ValueDeclarationLine>
    {
        public void Configure(EntityTypeBuilder<ValueDeclarationLine> entity)
        {
            entity.ToTable("ValueDeclarationLines");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ValueDeclarationHeaderId);

            entity.Property(e => e.Description).HasColumnType("varchar(150)").IsRequired();
            entity.Property(e => e.Brand).HasColumnType("varchar(40)");
            entity.Property(e => e.Model).HasColumnType("varchar(40)");
            entity.Property(e => e.Size).HasColumnType("varchar(20)");
            entity.Property(e => e.CountryOfOriginCode).HasColumnType("varchar(3)");
            entity.Property(e => e.UnitCode).HasColumnType("varchar(6)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Value).HasColumnType("decimal(14,2)");
            entity.Property(e => e.HsCode).HasColumnType("varchar(15)");
            entity.Property(e => e.Weight).HasColumnType("decimal(12,2)");

            entity.HasOne<ValueDeclarationHeader>()
                .WithMany()
                .HasForeignKey(e => e.ValueDeclarationHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
