using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CurrencyConversionConfig : IEntityTypeConfiguration<CurrencyConversion>
    {
        public void Configure(EntityTypeBuilder<CurrencyConversion> entity)
        {
            // NOTE (2026-08-09): HasKey must run BEFORE configuring Id's identity
            // strategy. Id is a genuine SQL Server IDENTITY column but is not part
            // of the primary key (the real PK is the FromCurrency/ToCurrency
            // composite below) - calling UseIdentityColumn() first lets EF's
            // implicit "Id" key-discovery convention treat Id as the tentative PK,
            // and the later HasKey() call then resets its value-generation
            // strategy back to "client-set" when it's demoted to a plain column.
            // That's exactly what caused "Cannot insert explicit value for
            // identity column ... IDENTITY_INSERT is set to OFF" on insert.
            // CurrencyExchangeConfig (same Id-is-identity-but-not-PK shape) already
            // does HasKey first, which is why only Conversion hit this.
            entity.HasKey(k => new { k.FromCurrency, k.ToCurrency });

            entity.Property(p => p.Id).UseIdentityColumn();

            entity.Property(p=>p.FromCurrency)               
                .IsRequired(true)
                .HasColumnType("nvarchar(3)").HasMaxLength(3);
            
            entity.Property(p=>p.ToCurrency).IsRequired(true).HasColumnType("nvarchar(3)").HasMaxLength(3);
            
            entity.Property(p => p.Value).HasColumnType("decimal(8,3)").IsRequired(true);
        }
    }
}
