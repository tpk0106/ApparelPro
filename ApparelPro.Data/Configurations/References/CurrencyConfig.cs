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
            currency.Property(x => x.Code)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnType("nvarchar");

            currency.HasOne<Country>()
            .WithOne()
            .IsRequired()
           //  .HasForeignKey<Currency>(c=>c.CountryCode);
            .HasForeignKey(typeof(Currency), @"CountryCode");


            // beolow code also same as above but need to test whether its working.
            // https://github.com/dotnet/EntityFramework.Docs/issues/3004
            currency.HasOne<Country>()
           .WithOne()
             .HasForeignKey<Currency>(c => c.CountryCode)
            .IsRequired(); // should come after HasForeignKey

            //currency.HasOne<Country>()
            //   .WithOne()
            //  // .HasPrincipalKey<Country>(x=>x.Code)
            //   .HasPrincipalKey<Currency>(x=>x.CountryCode)
            //   // .IsRequired()              
            //   .HasForeignKey<Country>(c=>c.Code);

            currency.Property(x => x.Id)
             .UseIdentityColumn();

            currency.Property(p => p.Name)
                .HasMaxLength(30)
                .HasColumnType("nvarchar");

            //  entity.HasOne(c => c.Country);            

            currency.Property(p => p.Minor)
              .HasMaxLength(3)
              .HasColumnType("nvarchar");
        }
    }
}
