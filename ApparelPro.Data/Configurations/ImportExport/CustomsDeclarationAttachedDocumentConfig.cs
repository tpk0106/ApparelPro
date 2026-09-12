using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class CustomsDeclarationAttachedDocumentConfig : IEntityTypeConfiguration<CustomsDeclarationAttachedDocument>
    {
        public void Configure(EntityTypeBuilder<CustomsDeclarationAttachedDocument> entity)
        {
            entity.ToTable("CustomsDeclarationAttachedDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CusNo).HasColumnType("varchar(15)").IsRequired();
            entity.HasIndex(e => e.CusNo);
            entity.Property(e => e.DocNo).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.DocTypeCode).HasColumnType("varchar(25)").IsRequired();
        }
    }
}
