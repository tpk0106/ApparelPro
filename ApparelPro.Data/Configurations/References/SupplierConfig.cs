using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class SupplierConfig : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> entity)
        {
            entity.HasKey(k => k.SupplierCode);

            entity.Property(p => p.SupplierCode)
                .IsRequired()
                .HasColumnType("int")
                .UseIdentityColumn();

            entity.Property(p => p.AddressId)
                .IsRequired(false)
                .HasColumnType("uniqueidentifier");
               // .HasDefaultValue(new Guid("00000000-0000-0000-0000-000000000000"));

            entity.Property<string>("Name")
                .IsRequired()
                .HasColumnType("nvarchar")
                .HasMaxLength(100);

            entity.Property(p => p.TelephoneNos)
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnType("nvarchar");

            entity.Property(p => p.MobileNos)
               .HasMaxLength(100)
               .IsRequired(false)
               .HasColumnType("nvarchar");

            entity.Property(p => p.Fax)
               .HasMaxLength(100)
               .IsRequired(false)
               .HasColumnType("nvarchar");

            entity.HasIndex(entity => entity.SupplierCode);

            //entity
            //   .HasMany(p => p.Addresses)
            //   .WithOne()
            //   .HasForeignKey(b => b.AddressId)
            //   .HasPrincipalKey(k => k.AddressId)
            //   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
