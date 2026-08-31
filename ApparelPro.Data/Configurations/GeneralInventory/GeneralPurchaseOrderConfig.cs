using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.GeneralInventory
{
    public class GeneralPurchaseOrderConfig : IEntityTypeConfiguration<GeneralPurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<GeneralPurchaseOrder> entity)
        {
            entity.ToTable("GeneralPurchaseOrders");
            entity.HasKey(e => e.PoNumber);
            entity.Property(e => e.PoNumber).HasColumnType("varchar(6)").HasColumnName("PoNumber").IsRequired();

            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode").IsRequired();
            entity.Property(e => e.OrderDate).HasColumnType("date").HasColumnName("OrderDate");
            entity.Property(e => e.OrderTime).HasColumnType("time").HasColumnName("OrderTime");
            entity.Property(e => e.BasisCode).HasColumnType("varchar(3)").HasColumnName("BasisCode");
            entity.Property(e => e.ProformaInvoiceNo).HasColumnType("varchar(15)").HasColumnName("ProformaInvoiceNo");
            entity.Property(e => e.ProformaInvoiceDate).HasColumnType("date").HasColumnName("ProformaInvoiceDate");
            entity.Property(e => e.CurrencyCode).HasColumnType("varchar(3)").HasColumnName("CurrencyCode");
            // Widened from legacy's varchar(6) USERID code - this app populates it from
            // User.Identity.Name (an email address), same varchar(50) width already used
            // for OrderwiseStockTransaction.CreatedByUsername for the same reason.
            entity.Property(e => e.UserId).HasColumnType("varchar(50)").HasColumnName("UserId");

            // CurrencyCode/BasisCode left unconstrained (nullable in source data - see sample
            // rows with blank CURR/BASIS) - same reasoning as SupplierPurchaseOrder's SupplierCode.
        }
    }
}
