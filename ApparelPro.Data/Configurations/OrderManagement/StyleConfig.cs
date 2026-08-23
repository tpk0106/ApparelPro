using ApparelPro.Data.Models.OrderManagement;
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

            // Fix (2026-08-16): was implicitly nvarchar(3) via convention. Now explicit varchar(3)
            // since Unit.Code narrowed to varchar(3) - EF's FK-type-inheritance convention was
            // about to silently flip this anyway; making it explicit avoids relying on that.
            entity.Property(p => p.Unit)
                .HasMaxLength(3)
                .HasColumnType("varchar")
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
                .HasMaxLength(30);  // Maps automatically to nvarchar(6)

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
                .HasMaxLength(30); // Maps automatically to nvarchar(6)

            entity.Property(p => p.Exported)
                .IsRequired(false)
                .HasDefaultValue(false)
                .HasColumnType("bit");

            // Tier 1 relationships audit (2026-08-16): Unit was previously enforced only by
            // matching values against Units, with no real database constraint.
            entity.HasOne<Unit>()
                .WithMany()
                .HasForeignKey(p => p.Unit)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);

            // Tier 2 relationships audit: (BuyerCode, Order) was previously enforced only by
            // matching values against PurchaseOrders (a Style could reference a nonexistent
            // order). PurchaseOrder's PK is already (BuyerCode, Order), so no HasPrincipalKey
            // override is needed here.
            entity.HasOne<PurchaseOrder>()
                .WithMany()
                .HasForeignKey(p => new { p.BuyerCode, p.Order })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
