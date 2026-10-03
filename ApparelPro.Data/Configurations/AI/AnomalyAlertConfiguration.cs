using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI;

// 🎓 WHAT: EF Core Fluent API config for the AnomalyAlerts table.
// WHY: Defines the SQL Server schema — column types, indexes, defaults.
//      Follows the same pattern as AiChatSessionConfiguration.cs:
//      Guid PK with NEWSEQUENTIALID(), GETUTCDATE() defaults, targeted indexes.

public class AnomalyAlertConfiguration : IEntityTypeConfiguration<AnomalyAlert>
{
    public void Configure(EntityTypeBuilder<AnomalyAlert> builder)
    {
        // 🎓 Table name follows the mandatory pluralisation rule from SKILL.md
        builder.ToTable("AnomalyAlerts");

        // ─── Primary Key ─────────────────────────────────────
        // 🎓 NEWSEQUENTIALID() avoids clustered index fragmentation on high-insert tables
        builder.HasKey(a => a.AlertId);

        builder.Property(a => a.AlertId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // ─── Classification Columns ──────────────────────────
        builder.Property(a => a.AnomalyType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasMaxLength(20);

        // ─── Entity Context Columns ──────────────────────────
        builder.Property(a => a.BuyerCode)
            .IsRequired();

        builder.Property(a => a.Order)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.TypeCode)
            .IsRequired();

        builder.Property(a => a.StyleCode)
            .IsRequired()
            .HasMaxLength(20);

        // 🎓 22-char composite ItemCode — same format across all inventory entities
        builder.Property(a => a.ItemCode)
            .IsRequired()
            .HasMaxLength(22);

        builder.Property(a => a.ItemDescription)
            .HasMaxLength(200);

        // ─── Anomaly Details ─────────────────────────────────
        // 🎓 decimal(18,4) gives enough precision for quantities and prices
        builder.Property(a => a.ExpectedValue)
            .HasColumnType("decimal(18,4)");

        builder.Property(a => a.ActualValue)
            .HasColumnType("decimal(18,4)");

        builder.Property(a => a.DeviationPercentage)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Unit)
            .HasMaxLength(10);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.RecommendedAction)
            .HasMaxLength(1000);

        // ─── User Interaction ────────────────────────────────
        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("NEW");

        builder.Property(a => a.AcknowledgedByUserId)
            .HasMaxLength(450);

        builder.Property(a => a.UserNote)
            .HasMaxLength(1000);

        // ─── Timestamps ──────────────────────────────────────
        builder.Property(a => a.DetectedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // ─── Deduplication ───────────────────────────────────
        builder.Property(a => a.Fingerprint)
            .IsRequired()
            .HasMaxLength(64);

        // ─── Indexes ─────────────────────────────────────────
        // 🎓 Primary query: "show me all NEW/unread alerts" — the notification bell
        //    needs this to be fast. Descending DetectedAt so newest alerts appear first.
        builder.HasIndex(a => new { a.Status, a.DetectedAt })
            .HasDatabaseName("IX_AnomalyAlerts_Status_Detected")
            .IsDescending(false, true);

        // 🎓 Entity lookup: "what anomalies exist for this style?" — used by
        //    AI chat context injection to find active alerts for the current entity.
        builder.HasIndex(a => new { a.BuyerCode, a.Order, a.TypeCode, a.StyleCode })
            .HasDatabaseName("IX_AnomalyAlerts_Style");

        // 🎓 Deduplication: scanner checks fingerprint before creating a new alert.
        //    Unique constraint prevents race conditions if two scanner runs overlap.
        builder.HasIndex(a => new { a.Fingerprint, a.Status })
            .HasDatabaseName("IX_AnomalyAlerts_Fingerprint_Status");

        // 🎓 Type filter: "show me all price spike alerts" — for the alert panel filters.
        builder.HasIndex(a => a.AnomalyType)
            .HasDatabaseName("IX_AnomalyAlerts_Type");
    }
}
