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

            // 🚀 Configure the One-to-Many Relationship
            entity.HasMany(b => b.Addresses)      // Bank has Many Addresses
                  .WithOne()                       // Address points back to one implicit parent
                  .HasForeignKey(a => a.BankCode)  // Enforces BankCode as the lookup key on the Address table
                  .HasPrincipalKey(b => b.BankCode)
                  .OnDelete(DeleteBehavior.Cascade); // If a bank is deleted, automatically delete its 3 addresses!

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnType("nvarchar");

            entity.Property(p => p.BankCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnType("nvarchar");      

            entity.Property(p => p.CurrencyCode)
               .IsRequired()
               .HasMaxLength(3)
               .HasColumnType("nvarchar");
            entity.Property(p => p.LoanLimit)              
               .HasColumnType("money");

            entity.Property(p => p.SwiftCode)
               .IsRequired()
               .HasMaxLength(11)
               .HasColumnType("nvarchar");

            entity.Property(p => p.TelephoneNos)               
               .HasMaxLength(50)
               .HasColumnType("nvarchar");
        }
    }
}
