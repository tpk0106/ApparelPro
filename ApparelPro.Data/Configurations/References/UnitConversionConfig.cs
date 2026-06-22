using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class UnitConversionConfig : IEntityTypeConfiguration<UnitConversion>
    {
        public void Configure(EntityTypeBuilder<UnitConversion> entity)
        {
            entity.ToTable("UnitConversion");
            entity.HasKey(e => new { e.FromUnit, e.ToUnit });
            entity.Property(e => e.FromUnit).IsRequired().HasColumnType("varchar(3)").HasColumnName("FromUnit");
            entity.Property(e => e.ToUnit).IsRequired().HasColumnType("varchar(3)").HasColumnName("ToUnit");
            entity.Property(e => e.Measure).IsRequired().HasColumnType("decimal(12,2)").HasColumnName("Measure");
        }
    }
}
