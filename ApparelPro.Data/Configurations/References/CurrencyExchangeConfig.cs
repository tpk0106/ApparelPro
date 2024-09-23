using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.References
{
    public class CurrencyExchangeConfig : IEntityTypeConfiguration<CurrencyExchange>
    {
        public void Configure(EntityTypeBuilder<CurrencyExchange> entity)
        {
            entity.HasKey(k => new { k.BaseCurrency, k.QuoteCurrency, k.ExchangeDate })
                .HasName("PK_CurencyExchange");   

            entity.Property(p=>p.Id).UseIdentityColumn();

            entity.Property(p => p.BaseCurrency)
                .IsRequired()
                .HasColumnType("nvarchar")
                .HasMaxLength(3);

            entity.Property(p => p.QuoteCurrency)
                .IsRequired()
                .HasColumnType("nvarchar")
                .HasMaxLength(3);

            entity.Property(p => p.ExchangeDate)
              .IsRequired()
              .HasColumnType("date");

            entity.Property(p => p.Rate)
               .IsRequired()
               .HasColumnType("decimal(12,3)");

        }
    }
}
