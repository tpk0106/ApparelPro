using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class PackingListLineConfig : IEntityTypeConfiguration<PackingListLine>
    {
        public void Configure(EntityTypeBuilder<PackingListLine> entity)
        {
            entity.ToTable("PackingListLines");
            entity.HasKey(e => new { e.InvoiceNumber, e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.NewOrder });

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)");
            entity.Property(e => e.Order).HasColumnType("varchar(12)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(12)");
            entity.Property(e => e.NewOrder).HasColumnType("varchar(12)");
            entity.Property(e => e.Detail).HasColumnType("nvarchar(max)");
        }
    }
}
