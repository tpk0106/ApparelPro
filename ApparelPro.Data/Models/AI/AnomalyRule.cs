using System.ComponentModel.DataAnnotations;

namespace ApparelPro.Data.Models.AI;

// 🎓 WHAT: Configurable thresholds for anomaly detection.
// WHY: Different factories/buyers/material types may have different tolerance
//      levels. Instead of hardcoding "10% over = LOW, 50% over = HIGH", these
//      rules let admins tune detection sensitivity per anomaly type.
//      E.g. expensive imported fabric might have tighter thresholds (5%)
//      while cheap local trim might tolerate 20% variance before alerting.

/// <summary>
/// Defines the detection thresholds for a specific anomaly type.
/// The AnomalyDetectionJob reads these rules to decide when to fire alerts
/// and at what severity level.
/// </summary>
public class AnomalyRule
{
    public int RuleId { get; set; }

    /// <summary>
    /// Which anomaly type this rule governs.
    /// "OVER_CONSUMPTION", "PRICE_SPIKE", "WASTE_DAMAGE"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AnomalyType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for this rule.
    /// E.g. "Default Over-Consumption Rule", "Fabric Price Spike Detection"
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description explaining the rule's purpose.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    // ─── Severity Thresholds (percentage deviation) ──────────
    // 🎓 These define the boundaries between severity levels.
    //    If deviation >= LowThreshold but < MediumThreshold → LOW
    //    If deviation >= MediumThreshold but < HighThreshold → MEDIUM
    //    If deviation >= HighThreshold but < CriticalThreshold → HIGH
    //    If deviation >= CriticalThreshold → CRITICAL
    //    Below LowThreshold → no alert generated.

    /// <summary>
    /// Minimum deviation percentage to trigger a LOW severity alert.
    /// Default: 10.0 (10% over expected)
    /// </summary>
    public decimal LowThreshold { get; set; } = 10.0m;

    /// <summary>
    /// Deviation percentage for MEDIUM severity.
    /// Default: 25.0 (25% over expected)
    /// </summary>
    public decimal MediumThreshold { get; set; } = 25.0m;

    /// <summary>
    /// Deviation percentage for HIGH severity.
    /// Default: 50.0 (50% over expected)
    /// </summary>
    public decimal HighThreshold { get; set; } = 50.0m;

    /// <summary>
    /// Deviation percentage for CRITICAL severity.
    /// Default: 100.0 (100% over expected — double the planned amount)
    /// </summary>
    public decimal CriticalThreshold { get; set; } = 100.0m;

    // ─── Scope Filters ───────────────────────────────────────
    // 🎓 Optional filters to apply this rule only to specific buyers/stock codes.
    //    Null means "apply to all". This allows different thresholds for
    //    different material categories (e.g. tighter for imported fabric).

    /// <summary>
    /// If set, this rule only applies to materials for this buyer.
    /// Null = applies to all buyers.
    /// </summary>
    public int? BuyerCode { get; set; }

    /// <summary>
    /// If set, this rule only applies to materials with this stock code prefix.
    /// E.g. "FB" for fabrics, "TR" for trims, "AC" for accessories.
    /// Null = applies to all stock codes.
    /// </summary>
    [MaxLength(4)]
    public string? StockCodeFilter { get; set; }

    // ─── Rule State ──────────────────────────────────────────

    /// <summary>
    /// Whether this rule is currently active. Inactive rules are skipped by the scanner.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order when listing rules in the admin UI.
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
