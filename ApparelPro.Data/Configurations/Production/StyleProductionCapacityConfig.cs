using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class StyleProductionCapacityConfig : IEntityTypeConfiguration<StyleProductionCapacity>
    {
        public void Configure(EntityTypeBuilder<StyleProductionCapacity> entity)
        {
            entity.ToTable("StyleProductionCapacities");

            entity.HasKey(e => new
            {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode
            });

            entity.Property(e => e.Order).HasColumnType("varchar(20)");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)");
            entity.Property(e => e.OutputPerDay).HasColumnType("decimal(12,2)");
        }
    }
}
