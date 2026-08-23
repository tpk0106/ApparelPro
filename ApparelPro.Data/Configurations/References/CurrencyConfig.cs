using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CurrencyConfig : IEntityTypeConfiguration<Currency>
    {
        public CurrencyConfig() { }

        public void Configure(EntityTypeBuilder<Currency> currency)
        {
            currency.HasKey(x => x.Code);
            // Fix (2026-08-16): was nvarchar, which mismatched the varchar(3) type used by
            // every table that references this column via FK (SupplierPurchaseOrders, etc.) -
            // see AddTier1ReferenceDataForeignKeys migration for the full narrowing steps.
            currency.Property(x => x.Code)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("varchar");

            //currency.HasOne<Bank>()
            //.WithOne()
            //.IsRequired()
            ////  .HasForeignKey<Currency>(c=>c.CountryCode);
            //.HasForeignKey(typeof(Currency), @"CountryCode");


            // beolow code also same as above but need to test whether its working.
            // https://github.com/dotnet/EntityFramework.Docs/issues/3004
            // currency.HasOne<Bank>()
            //.WithOne()
            //  .HasForeignKey<Currency>(c => c.CountryCode)
            // .IsRequired(); // should come after HasForeignKey

            //currency.HasOne<Bank>()
            //   .WithOne()
            //  // .HasPrincipalKey<Bank>(x=>x.Code)
            //   .HasPrincipalKey<Currency>(x=>x.CountryCode)
            //   // .IsRequired()              
            //   .HasForeignKey<Bank>(c=>c.Code);

            currency.Property(x => x.CountryCode)
                .HasMaxLength(3)
                .HasColumnType("nvarchar")
                .IsRequired();

            currency.Property(x => x.Id)
             .UseIdentityColumn();

            currency.Property(p => p.Name)
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            //  entity.HasOne(c => c.Bank);            

            currency.Property(p => p.Minor)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");

            // Tier 1 relationships audit (2026-08-16): CountryCode was previously enforced only
            // by matching values against Countries, with no real database constraint (see the
            // commented-out attempts above - none of them were ever actually wired up).
            // Country's PK IS its Code column, so no HasPrincipalKey override needed.
            currency.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
