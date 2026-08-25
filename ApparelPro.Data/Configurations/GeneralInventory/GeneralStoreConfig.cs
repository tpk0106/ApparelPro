using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.GeneralInventory
{
    public class GeneralStoreConfig : IEntityTypeConfiguration<GeneralStore>
    {
        public void Configure(EntityTypeBuilder<GeneralStore> entity)
        {
            entity.ToTable("GeneralStores");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("varchar(3)").HasColumnName("Code").IsRequired();
            entity.Property(e => e.Description).HasColumnType("varchar(30)").HasColumnName("Description").IsRequired();
        }
    }
}
