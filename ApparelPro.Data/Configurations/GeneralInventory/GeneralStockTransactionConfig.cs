using ApparelPro.Data.Models.GeneralInventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.GeneralInventory
{
    public class GeneralStockTransactionConfig : IEntityTypeConfiguration<GeneralStockTransaction>
    {
        public void Configure(EntityTypeBuilder<GeneralStockTransaction> entity)
        {
            entity.ToTable("GeneralStockTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.TransactionTypeCode).HasColumnType("varchar(3)").HasColumnName("TransactionTypeCode").IsRequired();
            entity.Property(e => e.DocumentNumber).HasColumnType("varchar(6)").HasColumnName("DocumentNumber").IsRequired();

            entity.Property(e => e.TransactionDate).HasColumnType("date").HasColumnName("TransactionDate").IsRequired();
            entity.Property(e => e.TransactionTime).HasColumnType("time").HasColumnName("TransactionTime");

            entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(10)").HasColumnName("InvoiceNumber");
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").HasColumnName("StoreCode").IsRequired();
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").HasColumnName("ItemCode").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").HasColumnName("Unit").IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").HasColumnName("Quantity").IsRequired();

            entity.Property(e => e.SupplierCode).HasColumnType("varchar(6)").HasColumnName("SupplierCode");
            entity.Property(e => e.Price).HasColumnType("decimal(12,4)").HasColumnName("Price").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").HasColumnName("Currency");
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(7,3)").HasColumnName("ExchangeRate");

            entity.Property(e => e.BuyerCode).HasColumnName("BuyerCode");
            entity.Property(e => e.Order).HasColumnType("varchar(12)").HasColumnName("Order");
            entity.Property(e => e.PoNumber).HasColumnType("varchar(6)").HasColumnName("PoNumber");
            entity.Property(e => e.DepartmentCode).HasColumnType("varchar(3)").HasColumnName("DepartmentCode");
            entity.Property(e => e.LinkedDocumentNumber).HasColumnType("varchar(6)").HasColumnName("LinkedDocumentNumber");

            entity.HasIndex(e => new { e.StoreCode, e.ItemCode, e.TransactionDate });

            // Currency/Unit left unconstrained here (nullable, and this ledger sees far more
            // volume/variety than the master tables) - same reasoning already applied to
            // ScheduledShipments.DestinationCode: FK added only where the schema gap is closed.
        }
    }
}
