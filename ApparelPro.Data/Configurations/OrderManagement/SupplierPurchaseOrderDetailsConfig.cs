using ApparelPro.Data.Models.OrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.OrderManagement
{
    public class SupplierPurchaseOrderDetailsConfig : IEntityTypeConfiguration<SupplierPurchaseOrderDetails>
    {
        public void Configure(EntityTypeBuilder<SupplierPurchaseOrderDetails> entity)
        {
            entity.ToTable("SupplierPurchaseOrderDetails");

            // 1. FIXED PRIMARY KEY: Composite key includes PONo and ItemCode
            // This safely allows multiple different material line items to exist under one PO!
            entity.HasKey(k => new { k.PONumber, k.Buyer, k.Order, k.Type, k.Style, k.ItemCode });

            // 2. CRITICAL FIX: Explicitly ignore the Id property so EF never tries to query or save it!
            entity.Ignore(e => e.Id);


            // 2. THE PERMANENT MAP FIX: Links your C# class property "PONumber"
            // straight to your physical SQL Server database column "PONo" safely!
            entity.Property(e => e.PONumber)
                .HasColumnType("varchar(10)")
                .HasColumnName("PONumber") // Maps directly to your physical DB column name!
                .IsRequired();

            entity.Property(p => p.Buyer)
                .HasColumnType("int")
                .HasColumnName("Buyer")
                .IsRequired();

            entity.Property(p => p.Order)
                .HasColumnType("varchar(12)") // FIXED: Clean varchar with fixed width bounds
                .HasColumnName("Order")
                .IsRequired();

            entity.Property(p => p.Type)
                .HasColumnType("int")
                .HasColumnName("Type")
                .IsRequired();

            entity.Property(p => p.Style)
                .HasColumnType("varchar(12)")
                .HasColumnName("Style")
                .IsRequired();

            // 3. SECURED ITEM STRING EXTENSION: Increased to varchar(40)
            // This safely accommodates your long 22+ character dynamic item configurations
            entity.Property(p => p.ItemCode)
                .HasColumnType("varchar(40)")
                .HasColumnName("ItemCode")
                .IsRequired();

            // FIXED (2026-08-07): widened from varchar(10) to varchar(23), then to varchar(30)
            // per the user's follow-up request - a real Supplier PO save had thrown "String or
            // binary data would be truncated ... column 'RefNo'" for an operator-entered
            // reference value longer than 10 chars. The frontend "Reference Number" field is
            // now capped at 30 chars to match (see supplier-purchase-order-workspace.tsx).
            entity.Property(p => p.RefNo)
                .HasColumnType("varchar(30)")
                .HasColumnName("RefNo");

            entity.Property(p => p.OrderUnit)
                .HasColumnType("varchar(3)")
                .HasColumnName("OrderUnit")
                .IsRequired();

            entity.Property(p => p.OrderQuantity)
                .HasColumnType("decimal(12,2)")
                .HasColumnName("OrderQuantity")
                .IsRequired();

            // 4. FIXED FINANCIAL PRECISION: Scaled to 4 decimal places matching legacy data (e.g. 1.5900)
            entity.Property(p => p.UnitPrice)
                .HasColumnType("decimal(10,4)")
                .HasColumnName("UnitPrice")
                .IsRequired();

            entity.Property(p => p.ExportDate)
                .HasColumnType("datetime")
                .HasColumnName("ExportDate");

            entity.Property(p => p.LCNo)
                .HasColumnType("varchar(23)")
                .HasColumnName("LCNo");

            entity.Property(e => e.Balance)
                .HasColumnType("decimal(12,2)")
                .HasColumnName("Balance")
                .IsRequired();
        }
    }
}
