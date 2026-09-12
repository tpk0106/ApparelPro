using ApparelPro.Data.Models.ImportExport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.ImportExport
{
    public class PackingListCartonDetailConfig : IEntityTypeConfiguration<PackingListCartonDetail>
    {
        public void Configure(EntityTypeBuilder<PackingListCartonDetail> entity)
        {
            entity.ToTable("PackingListCartonDetails");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(25)").IsRequired();
            entity.Property(e => e.Order).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.StyleCode).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.NewOrder).HasColumnType("varchar(12)").IsRequired();
            entity.HasIndex(e => new { e.InvoiceNumber, e.BuyerCode, e.Order, e.TypeCode, e.StyleCode, e.NewOrder });

            entity.Property(e => e.Color).HasColumnType("varchar(6)");
            entity.Property(e => e.Size).HasColumnType("varchar(6)");
            entity.Property(e => e.Qty).HasColumnType("decimal(10,2)");
        }
    }
}
