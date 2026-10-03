using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI;

// 🎓 WHAT: EF Core Fluent API config for the AnomalyRules table.
// WHY: Stores configurable detection thresholds that the AnomalyDetectionJob
//      reads at scan time. Seeded with sensible defaults so the system works
//      out of the box — admins can tune later.

public class AnomalyRuleConfiguration : IEntityTypeConfiguration<AnomalyRule>
{
    public void Configure(EntityTypeBuilder<AnomalyRule> builder)
    {
        // 🎓 Pluralised table name per SKILL.md convention
        builder.ToTable("AnomalyRules");

        builder.HasKey(r => r.RuleId);

        // 🎓 Identity seed — simple int PK since rules are low-volume admin data
        builder.Property(r => r.RuleId)
            .UseIdentityColumn();

        // ─── Core Fields ─────────────────────────────────────
        builder.Property(r => r.AnomalyType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.RuleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        // ─── Thresholds ──────────────────────────────────────
        // 🎓 decimal(18,2) — percentage values (e.g. 10.00, 25.50)
        builder.Property(r => r.LowThreshold)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(10.0m);

        builder.Property(r => r.MediumThreshold)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(25.0m);

        builder.Property(r => r.HighThreshold)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(50.0m);

        builder.Property(r => r.CriticalThreshold)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(100.0m);

        // ─── Scope Filters ───────────────────────────────────
        builder.Property(r => r.StockCodeFilter)
            .HasMaxLength(4);

        // ─── State ───────────────────────────────────────────
        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.SortOrder)
            .HasDefaultValue(0);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // ─── Indexes ─────────────────────────────────────────
        // 🎓 The scanner queries active rules by anomaly type at each scan cycle
        builder.HasIndex(r => new { r.AnomalyType, r.IsActive })
            .HasDatabaseName("IX_AnomalyRules_Type_Active");

        // ─── Seed Data ───────────────────────────────────────
        // 🎓 Three default rules — one per anomaly type. These provide sensible
        //    out-of-the-box thresholds. Admins can modify or add buyer/stock-specific rules later.
        builder.HasData(
            new AnomalyRule
            {
                RuleId = 1,
                AnomalyType = "OVER_CONSUMPTION",
                RuleName = "Default Over-Consumption Detection",
                Description = "Flags when actual issued quantity exceeds planned consumption plus percentage allowance. " +
                              "Compares OrderwiseStockMaster.IssuedQuantity against StyleMaterialCostProfile.TotalConsumption " +
                              "adjusted by StyleMaterialConsumptionLedger.PercentageAllowance.",
                LowThreshold = 10.0m,
                MediumThreshold = 25.0m,
                HighThreshold = 50.0m,
                CriticalThreshold = 100.0m,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new AnomalyRule
            {
                RuleId = 2,
                AnomalyType = "PRICE_SPIKE",
                RuleName = "Default Price Spike Detection",
                Description = "Flags when a material's current unit price in StyleMaterialCostProfile significantly " +
                              "exceeds the historical average price for the same ItemCode across all styles. " +
                              "Uses rolling average of UnitPrice grouped by ItemCode.",
                LowThreshold = 15.0m,
                MediumThreshold = 30.0m,
                HighThreshold = 60.0m,
                CriticalThreshold = 120.0m,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new AnomalyRule
            {
                RuleId = 3,
                AnomalyType = "WASTE_DAMAGE",
                RuleName = "Default Waste & Damage Detection",
                Description = "Flags when damaged or supplier-returned quantities in OrderwiseStockMaster exceed " +
                              "a percentage of the total ordered quantity. DamagedQuantity + SupplierReturnQuantity " +
                              "compared against OrderedQuantity.",
                LowThreshold = 5.0m,
                MediumThreshold = 10.0m,
                HighThreshold = 20.0m,
                CriticalThreshold = 40.0m,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
