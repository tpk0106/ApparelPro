using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class DocumentTypeConfig : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> entity)
        {
            entity.ToTable("DocumentTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocNo).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.DocTypeCode).HasColumnType("varchar(25)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("varchar(40)").IsRequired();
            entity.HasIndex(e => new { e.DocNo, e.DocTypeCode }).IsUnique();
        }
    }
}
