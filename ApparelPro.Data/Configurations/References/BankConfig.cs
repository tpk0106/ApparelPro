using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class BankConfig : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> entity)
        {
            entity.HasKey(x => x.BankCode);
            entity.Property(p => p.BankCode).ValueGeneratedNever();
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.BankCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");

            //entity.Property(p => p.AddressId)              
            //   .HasColumnType("int");

            // entity.HasMany(e=>e.Addresses);

            entity.Property(p => p.CurrencyCode)
               .IsRequired()
               .HasMaxLength(3)
               .HasColumnType("nvarchar");
            entity.Property(p => p.LoanLimit)
               //  .IsRequired()
               // .HasMaxLength(3)
               .HasColumnType("money");

            entity.Property(p => p.SwiftCode)
               //  .IsRequired()
               .HasMaxLength(3)
               .HasColumnType("nvarchar");

            entity.Property(p => p.TelephoneNos)
               // .IsRequired()
               .HasMaxLength(50)
               .HasColumnType("nvarchar");
        }
    }
}
