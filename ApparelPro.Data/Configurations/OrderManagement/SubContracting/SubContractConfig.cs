using ApparelPro.Data.Models.OrderManagement.SubContracting;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement.SubContracting
{
    public class SubContractConfig : IEntityTypeConfiguration<SubContract>
    {
        public void Configure(EntityTypeBuilder<SubContract> entity)
        {
            entity.ToTable("SubContracts");

            entity.HasKey(e => new {
                e.BuyerCode,
                e.Order,
                e.TypeCode,
                e.StyleCode,
                e.SubContractorCode
            });

            entity.Property(e => e.BuyerCode).HasColumnType("int").HasColumnName("Buyer");
            entity.Property(e => e.Order).HasColumnType("varchar(20)").HasColumnName("Order");
            entity.Property(e => e.TypeCode).HasColumnType("int").HasColumnName("Type");
            entity.Property(e => e.StyleCode).HasColumnType("varchar(20)").HasColumnName("Style");
            entity.Property(e => e.SubContractorCode).HasColumnType("varchar(6)").HasColumnName("SubContractorCode");

            entity.Property(e => e.SubQuantity).HasColumnType("decimal(9,0)").HasColumnName("SubQuantity");
            entity.Property(e => e.CostPerGarment).HasColumnType("decimal(11,2)").HasColumnName("CostPerGarment");
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency");
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(9,0)").HasColumnName("ReceivedQuantity");

            // Tier 1 relationships audit (2026-08-16): these 2 columns were previously enforced
            // only by matching values, with no real database constraint.
            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(e => e.Currency)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(e => e.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
