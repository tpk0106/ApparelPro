using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class AdditionalCostConfig : IEntityTypeConfiguration<AdditionalCost>
    {
        public void Configure(EntityTypeBuilder<AdditionalCost> entity)
        {
            entity.ToTable("AdditionalCosts");
            entity.HasKey(e => e.Code);

            entity.Property(e => e.Code).HasColumnType("varchar(3)").HasColumnName("Code");
            entity.Property(e => e.Description).HasColumnType("varchar(40)").HasColumnName("Description");
        }
    }
}
