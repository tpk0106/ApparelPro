using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CountryConfig : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> entity)
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");

            entity.Property(e => e.Id)
                .UseIdentityColumn();

            entity.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(30)
               .HasColumnType("nvarchar");

            entity.Property(p => p.Flag)
                .HasColumnType("varbinary(MAX)");
        }
    }
}
