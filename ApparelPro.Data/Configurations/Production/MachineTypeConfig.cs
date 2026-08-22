using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class MachineTypeConfig : IEntityTypeConfiguration<MachineType>
    {
        public void Configure(EntityTypeBuilder<MachineType> entity)
        {
            entity.ToTable("MachineTypes");
            entity.HasKey(x => x.Code);
            entity.Property(p => p.Code)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
