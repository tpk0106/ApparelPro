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

            // Fix (2026-08-16): was nvarchar, which matched Currency.Code's old type. Now that
            // Currency.Code narrowed to varchar(3), this narrows too so
            // FK_Banks_Currencies_CurrencyCode doesn't hit a type mismatch.
            entity.Property(p => p.CurrencyCode)
               .IsRequired()
               .HasMaxLength(3)
               .HasColumnType("varchar");
            entity.Property(p => p.LoanLimit)              
               .HasColumnType("money");

            entity.Property(p => p.SwiftCode)
               .IsRequired()
               .HasMaxLength(11)
               .HasColumnType("nvarchar");

            entity.Property(p => p.TelephoneNos)
               .HasMaxLength(50)
               .HasColumnType("nvarchar");

            // Tier 1 relationships audit (2026-08-16): CurrencyCode was previously enforced only
            // by matching values against Currencies, with no real database constraint.
            // Currency's PK IS its Code column, so no HasPrincipalKey override needed.
            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(p => p.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
