using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class StyleComponentBreakdownConfig : IEntityTypeConfiguration<StyleComponentBreakdown>
    {
        public void Configure(EntityTypeBuilder<StyleComponentBreakdown> entity)
        {
            entity.ToTable("StyleComponentBreakdowns");

            entity.HasKey(e => new
            {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.ComponentSequence
            });

            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.ComponentCode).HasColumnType("nvarchar(4)").IsRequired();

            entity.HasOne<GarmentComponent>()
                .WithMany()
                .HasForeignKey(e => e.ComponentCode)
                .HasPrincipalKey(c => c.ComponentCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
