
using ApparelPro.Data.Models.OrderwiseInventory;
using ApparelPro.Data.Models.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApparelPro.Data.Configurations.OrderwiseInventory
{
    public class OrderwiseStockTransactionConfig:IEntityTypeConfiguration<OrderwiseStockTransaction>
    {
        public void Configure(EntityTypeBuilder<OrderwiseStockTransaction> entity)
        {
            entity.ToTable("OrderwiseStockTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            // Unique index compound constraint to track exact transaction lines cleanly.
            // Uses StoreCode (not StockCode) to match legacy IN_STTR's per-line uniqueness key
            // (buyer+order+item_cd+store_cd, confirmed against IN_GIN3.PRG's seek pattern) — StockCode
            // is already embedded as the first 2 chars of the 22-char ItemCode, so including it here
            // instead of StoreCode was a latent bug: two lines with the same item pulled from two
            // different stores/bases within one document would incorrectly collide on this constraint.
            entity.HasIndex(e => new { e.DocumentNumber, e.TransactionType, e.StoreCode, e.ItemCode }).IsUnique();

            entity.Property(e => e.DocumentNumber).HasColumnType("varchar(10)").IsRequired();
            entity.Property(e => e.TransactionType).HasColumnType("varchar(2)").IsRequired();
            // Order confirmed at 12 chars against IN_STTR.DBF (ORDER C(12)) — already correct.
            entity.Property(e => e.Order).HasColumnType("varchar(12)").IsRequired();
            entity.Property(e => e.DepartmentCode).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.StockCode).HasColumnType("varchar(2)").IsRequired();
            entity.Property(e => e.StoreCode).HasColumnType("varchar(3)").IsRequired();
            // WIDENED from varchar(6): ItemCode is the full 22-char composite key (verified against IN_STTR.DBF: ITEM_CD C(22))
            entity.Property(e => e.ItemCode).HasColumnType("varchar(22)").IsRequired();
            entity.Property(e => e.Unit).HasColumnType("varchar(3)").IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.CreatedByUsername).HasColumnType("varchar(50)").IsRequired();
            entity.Property(e => e.TransactionDate).HasColumnType("date").IsRequired();

            // GIN traceability additions
            entity.Property(e => e.BalanceToReceive).HasColumnType("decimal(12,2)").IsRequired().HasDefaultValue(0m);
            entity.Property(e => e.SourceDocumentNumber).HasColumnType("varchar(10)").IsRequired(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10,4)").IsRequired(false);
            entity.Property(e => e.Currency).HasColumnType("varchar(3)").IsRequired(false);

            // GTN traceability additions — counterpart Buyer/Order for a Transfer-Out ('6T')
            // or Transfer-In ('1T') row (legacy t_buyer/t_order).
            entity.Property(e => e.CounterpartyBuyerCode).HasColumnType("int").IsRequired(false);
            entity.Property(e => e.CounterpartyOrder).HasColumnType("varchar(12)").IsRequired(false);

            // SRN traceability addition — the Supplier a Supplier Return Note ('7S') row
            // was returned to (legacy supp_cd).
            entity.Property(e => e.SupplierCode).HasColumnType("int").IsRequired(false);

            // AIN traceability additions — Sub-Contractor code (varchar(6), matches
            // SubContractor.Code) and Additional Process code (varchar(3), matches
            // AdditionalCost.Code) for an Additional Issue Note ('4X') row.
            entity.Property(e => e.SubContractorCode).HasColumnType("varchar(6)").IsRequired(false);
            entity.Property(e => e.AdditionalProcessCode).HasColumnType("varchar(3)").IsRequired(false);

            // Tier 1 relationships audit (2026-08-16): these 5 columns were previously enforced
            // only by matching values, with no real database constraint. DepartmentCode ->
            // Departments.DepartmentCode is deliberately EXCLUDED here - 17 existing rows have
            // a DepartmentCode with no matching Department row, so that one needs a data-cleanup
            // decision before it can be added.
            entity.HasOne<Stock>()
                .WithMany()
                .HasForeignKey(e => e.StockCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Supplier>()
                .WithMany()
                .HasForeignKey(e => e.SupplierCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<SubContractor>()
                .WithMany()
                .HasForeignKey(e => e.SubContractorCode)
                .OnDelete(DeleteBehavior.Restrict);

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
