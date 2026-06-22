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

            entity.HasIndex(p => new { p.BuyerCode, p.Order, p.TypeCode, p.StyleCode })
                .IsUnique();

            entity.Property(p => p.BuyerCode)
                .HasMaxLength(6)
                .HasColumnName("Buyer")
                .IsRequired()
                .HasColumnType("int");

            entity.Property(p => p.Order)
                .HasMaxLength(12)
                .IsRequired(); // Maps automatically to nvarchar(12)

            entity.Property(p => p.TypeCode)
                .IsRequired()
                .HasColumnName("Type")
                .HasColumnType("int");

            entity.Property(p => p.StyleCode)
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnName("Style");  // Maps automatically to nvarchar(12)              

            entity.Property(p => p.ColorRatio)
                .HasMaxLength(1)
                .IsRequired(false)
                .HasColumnName("ColorRatio");  // Maps automatically to nvarchar(1)              

            entity.Property(p => p.SizeRatio)
                .HasMaxLength(1)
                .IsRequired(false)
                .HasColumnName("SizeRatio");

            // OrderDate remains required (assumes it is filled on creation)
            entity.Property(p => p.OrderDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(p => p.Unit)
                .HasMaxLength(3) // Maps automatically to nvarchar(3)
                .IsRequired(false);                

            entity.Property(p => p.Quantity)
                .IsRequired(false)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.UnitPrice)
                .IsRequired(false)
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.ExportBalance)
                .IsRequired(false)
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
                .HasMaxLength(6);  // Maps automatically to nvarchar(6)

            // These three are now explicitly configured as optional (nullable)
            entity.Property(p => p.ApprovedDate)
                .HasColumnType("date")
                .IsRequired(false); // Allows NULL in database until production phase

            entity.Property(p => p.ProductionEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(p => p.EstimateApprovalDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(p => p.EstimateApprovalUserName)
                .IsRequired(false)
                .HasMaxLength(6); // Maps automatically to nvarchar(6)

            entity.Property(p => p.Exported)
                .IsRequired(false)
                .HasDefaultValue(false)
                .HasColumnType("bit");
        }
    }
}
