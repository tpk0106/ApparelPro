using System.ComponentModel.DataAnnotations;

namespace ApparelPro.Data.Models.AI;

// 🎓 WHAT: Represents a single anomaly detected by the background scanner.
// WHY: Phase 3 (Anomaly Detection & Alerts) needs persistent storage for
//      detected issues — over-consumption, price spikes, and waste/damage —
//      so the notification bell can show them, the AI chat can reference them,
//      and users can acknowledge/dismiss them without losing the audit trail.

/// <summary>
/// A detected anomaly in material consumption data. Created by the
/// AnomalyDetectionJob background scanner and surfaced through the
/// notification bell + AI chat context.
/// </summary>
public class AnomalyAlert
{
    // 🎓 Guid PK with NEWSEQUENTIALID() — same pattern as AiChatSession.
    //    Sequential GUIDs avoid index fragmentation on SQL Server clustered indexes.
    public Guid AlertId { get; set; }

    // ─── Classification ──────────────────────────────────────

    /// <summary>
    /// The category of anomaly detected.
    /// "OVER_CONSUMPTION" — actual issued exceeds planned + allowance
    /// "PRICE_SPIKE"      — unit price significantly above historical average
    /// "WASTE_DAMAGE"     — damaged/returned quantities exceed normal thresholds
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AnomalyType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level: "LOW", "MEDIUM", "HIGH", "CRITICAL".
    /// Determined by how far the actual value deviates from the expected range.
    /// LOW = 10-25% deviation, MEDIUM = 25-50%, HIGH = 50-100%, CRITICAL = >100%
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty;

    // ─── Entity Context ──────────────────────────────────────
    // 🎓 Links the alert to the specific material/style/order it relates to.
    //    Uses the same Buyer/Order/Style composite key pattern as the rest of ApparelPro.

    public int BuyerCode { get; set; }

    [Required]
    [MaxLength(10)]
    public string Order { get; set; } = null!;

    public int TypeCode { get; set; }

    [Required]
    [MaxLength(20)]
    public string StyleCode { get; set; } = null!;

    /// <summary>
    /// The 22-char composite ItemCode (StockCode + ItemCode + Feature1-4).
    /// Same format as StyleMaterialCostProfile.ItemCode and OrderwiseStockMaster.ItemCode.
    /// </summary>
    [Required]
    [MaxLength(22)]
    public string ItemCode { get; set; } = null!;

    /// <summary>
    /// Human-readable material description (copied from StyleMaterialCostProfile.Description
    /// at detection time for display without extra joins).
    /// </summary>
    [MaxLength(200)]
    public string? ItemDescription { get; set; }

    // ─── Anomaly Details ─────────────────────────────────────
    // 🎓 The actual numbers behind the anomaly — what was expected vs what happened.
    //    Stored as decimals so the notification panel and AI chat can display them
    //    without re-querying the source tables.

    /// <summary>
    /// The expected/planned value (e.g. planned consumption + allowance, average price, normal damage rate).
    /// </summary>
    public decimal ExpectedValue { get; set; }

    /// <summary>
    /// The actual observed value that triggered the alert.
    /// </summary>
    public decimal ActualValue { get; set; }

    /// <summary>
    /// Percentage deviation from expected: ((Actual - Expected) / Expected) * 100.
    /// Positive = over, Negative = under.
    /// </summary>
    public decimal DeviationPercentage { get; set; }

    /// <summary>
    /// Unit of measure for ExpectedValue/ActualValue (e.g. "YDS", "PCS", "USD").
    /// </summary>
    [MaxLength(10)]
    public string? Unit { get; set; }

    /// <summary>
    /// AI-generated plain-English explanation of the anomaly.
    /// E.g. "Fabric XYZ issued 340 YDS against planned 200 YDS (+70% over budget including 5% allowance)"
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// AI-generated recommended action.
    /// E.g. "Review cutting room waste reports. Consider re-negotiating supplier allowance."
    /// </summary>
    [MaxLength(1000)]
    public string? RecommendedAction { get; set; }

    // ─── User Interaction ────────────────────────────────────
    // 🎓 Tracks how the user responds to the alert — read it, dismissed it, or resolved it.

    /// <summary>
    /// Current status: "NEW", "READ", "ACKNOWLEDGED", "RESOLVED", "DISMISSED".
    /// NEW = just detected, READ = user saw it, ACKNOWLEDGED = user noted it,
    /// RESOLVED = underlying issue fixed, DISMISSED = user chose to ignore.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "NEW";

    /// <summary>
    /// The user who acknowledged/resolved/dismissed the alert (from JWT NameIdentifier).
    /// Null until someone acts on it.
    /// </summary>
    [MaxLength(450)]
    public string? AcknowledgedByUserId { get; set; }

    /// <summary>
    /// When the user acted on the alert.
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Optional note the user adds when acknowledging/resolving.
    /// E.g. "Checked with cutting room — marker was wrong, corrected."
    /// </summary>
    [MaxLength(1000)]
    public string? UserNote { get; set; }

    // ─── Timestamps ──────────────────────────────────────────

    /// <summary>
    /// When the anomaly was first detected by the background scanner.
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// When the alert record was last updated (status change, user note, etc.).
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // ─── Deduplication ───────────────────────────────────────
    // 🎓 The background scanner runs periodically. Without deduplication, it would
    //    create duplicate alerts for the same ongoing issue. This fingerprint is a
    //    hash of (AnomalyType + BuyerCode + Order + TypeCode + StyleCode + ItemCode)
    //    so the scanner can check "does an active alert already exist for this exact issue?"

    /// <summary>
    /// SHA-256 hash of the anomaly's key dimensions for deduplication.
    /// Computed as: SHA256(AnomalyType|BuyerCode|Order|TypeCode|StyleCode|ItemCode)
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Fingerprint { get; set; } = string.Empty;
}
