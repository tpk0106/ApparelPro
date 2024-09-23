using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CurrencyConversionConfig : IEntityTypeConfiguration<CurrencyConversion>
    {
        public void Configure(EntityTypeBuilder<CurrencyConversion> entity)
        {
            entity.Property(p => p.Id).UseIdentityColumn();

            entity.HasKey(k => new { k.FromCurrency, k.ToCurrency });

            entity.Property(p=>p.FromCurrency)               
                .IsRequired(true)
                .HasColumnType("nvarchar(3)").HasMaxLength(3);
            
            entity.Property(p=>p.ToCurrency).IsRequired(true).HasColumnType("nvarchar(3)").HasMaxLength(3);
            
            entity.Property(p => p.Value).HasColumnType("decimal(8,3)").IsRequired(true);
        }
    }
}
