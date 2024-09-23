using ApparelPro.Data.Models.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class PODetailsConfig : IEntityTypeConfiguration<PODetails>
    {
        public void Configure(EntityTypeBuilder<PODetails> entity)
        {
            entity.HasKey(k => new { k.Buyer, k.Order, k.Type, k.Style });

            entity.Property(p => p.PONo)
               .IsRequired()
               .UseIdentityColumn()
               .HasColumnType("int");

            entity.Property(p => p.Buyer)
               .IsRequired()
               .ValueGeneratedNever()
               .HasColumnType("int");

            entity.Property(p => p.Order)
               .ValueGeneratedNever()
              .HasMaxLength(12)
              .IsRequired()
              .HasColumnType("nvarchar");

            entity.Property(p => p.Type)  
               .ValueGeneratedNever()
              .IsRequired()
              .HasColumnType("int");

            entity.Property(p => p.Style)
                .ValueGeneratedNever()
              .HasMaxLength(12)
              .HasColumnType("nvarchar");

            entity.Property(p => p.ItemCode)
              .HasMaxLength(22)
              .HasColumnType("nvarchar");

            entity.Property(p => p.RefNo)
             .HasMaxLength(10)
             .HasColumnType("nvarchar");

            entity.Property(p => p.OrderUnit)
             .HasMaxLength(3)
             .HasColumnType("nvarchar");

            entity.Property(p => p.OrderQuantity)           
             .HasColumnType("decimal(10,2)");

            entity.Property(p => p.UnitPrice)             
             .HasColumnType("decimal(10,2)");

            entity.Property(p => p.ExportDate)             
             .HasColumnType("datetime");

            entity.Property(p => p.LCNo)
             .HasMaxLength(23)
             .HasColumnType("nvarchar");

            entity.Property(p => p.Balance)            
             .HasColumnType("int");
        }
    }
}
