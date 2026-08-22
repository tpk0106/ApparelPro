using ApparelPro.Data.Models.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.Production
{
    public class OperationConfig : IEntityTypeConfiguration<Operation>
    {
        public void Configure(EntityTypeBuilder<Operation> entity)
        {
            entity.ToTable("Operations");
            entity.HasKey(x => x.OperationCode);
            entity.Property(p => p.OperationCode)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(4)
                .HasColumnType("nvarchar");
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar");
        }
    }
}
