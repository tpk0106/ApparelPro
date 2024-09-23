using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class StyleConfig : IEntityTypeConfiguration<Style>
    {
        public void Configure(EntityTypeBuilder<Style> entity)
        {
            entity.Property(p => p.Id)
                .UseIdentityColumn()                
                .IsRequired().HasColumnType("int");

            entity.HasKey(p => p.Id);

           // entity.HasKey(k => new { k.BuyerCode, k.Order, k.TypeCode, k.StyleCode });
            entity.HasIndex(p => new { p.BuyerCode, p.Order, p.TypeCode, p.StyleCode })
                .IsUnique();

            entity.Property(p => p.BuyerCode)
               .HasMaxLength(6)
               .HasColumnName("Buyer")
              .IsRequired()
              .HasColumnType("int");

            entity.Property(p => p.Order)
                .HasMaxLength(12)
               .IsRequired()
               .HasColumnType("nvarchar");         

            entity.Property(p => p.TypeCode)
              .IsRequired()
              .HasColumnName("Type")            
              .HasColumnType("int");

            entity.Property(p => p.StyleCode)
              .IsRequired()
              .HasMaxLength(12)
              .HasColumnName("Style")
              .HasColumnType("nvarchar");

            entity.Property(p => p.OrderDate)
          .HasColumnType("datetime");

            entity.Property(p => p.Unit)
              .HasMaxLength(3)
              .IsRequired(false)
              .HasColumnType("nvarchar");

            entity.Property(p => p.Quantity).IsRequired(false)                
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.UnitPrice).IsRequired(false)              
              .HasColumnType("decimal(10,2)");

            entity.Property(p => p.ExportBalance).IsRequired(false)
             .HasColumnType("decimal(10,2)");

            entity.Property(p => p.CustomerReturn)
               .IsRequired(false)
              .HasDefaultValue(false)
             .HasColumnType("bit");

            entity.Property(p => p.SupplierReturn)
                .IsRequired(false)
            .HasDefaultValue(false)
           .HasColumnType("bit");

            entity.Property(p => p.Username)
                .IsRequired(false)
           .HasMaxLength(6)
           .HasColumnType("nvarchar");

            entity.Property(p => p.ApprovedDate)
                //.IsRequired(false)
                .HasColumnType("date").HasDefaultValue(null);
                //.HasDefaultValueSql("getdate()"); ;

            entity.Property(p => p.ProductionEndDate)
               // .IsRequired(false)
           .HasColumnType("date").HasDefaultValue(null);
            //.HasDefaultValueSql("getdate()"); ;

            entity.Property(p => p.EstimateApprovalDate)
               // .IsRequired(false)
           .HasColumnType("date").HasDefaultValue(null);
            // .HasDefaultValueSql("getdate()"); ;

            entity.Property(p => p.EstimateApprovalUserName)
            .IsRequired(false)
           .HasMaxLength(6)
           .HasColumnType("nvarchar");

            entity.Property(p => p.Exported)
                .IsRequired(false)
                .HasDefaultValue(false)
           .HasColumnType("bit");
        }
    }
}
