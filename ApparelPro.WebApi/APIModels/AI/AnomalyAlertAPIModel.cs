// ═══════════════════════════════════════════════════════════════════════════
//  AnomalyAlertAPIModel.cs — Controller Layer Models for Anomaly Alerts
//  Location: ApparelPro.WebApi/APIModels/AI/AnomalyAlertAPIModel.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS THIS FILE?
// API models for the Phase 3 Anomaly Detection system. These are the JSON
// contracts the React frontend sends and receives.
//
// 🎓 WHY IS THIS SIMPLER THAN SOP MODELS?
// Anomaly alerts are SYSTEM-GENERATED — users never create or fully edit them.
// The only user action is updating the STATUS (acknowledge, resolve, dismiss).
// So we only need:
//   • AnomalyAlertAPIModel — read/response model (full details for the panel)
//   • UpdateAlertStatusAPIModel — status update request (minimal: just new status + note)
//   • AnomalyAlertSummaryAPIModel — compact model for the notification list
//
// 🎓 NO Create model needed — alerts come from AnomalyDetectionService scans.
// 🎓 NO Service model layer needed — AnomalyDetectionService lives in
//    ApparelPro.AI and returns AnomalyAlert entities directly (same pattern
//    as AiChatService). AutoMapper maps Entity → APIModel in one step.
//
// 🎓 AUTOMAPPER MAPPINGS (wired in ServicetoAPIModelMappings.cs):
//   CreateMap<AnomalyAlert, AnomalyAlertAPIModel>().MaxDepth(2);
//   CreateMap<AnomalyAlert, AnomalyAlertSummaryAPIModel>().MaxDepth(2);
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.WebApi.APIModels.AI
{
    // ─────────────────────────────────────────────────────────────────────
    //  FULL READ MODEL (returned by GET /api/anomaly/alerts and detail views)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Full anomaly alert — returned in the notification panel and detail views.
    /// Contains all information the frontend needs to display and act on an alert.
    ///
    /// 🎓 FIELD GROUPS:
    ///   • Identity: AlertId (for status updates)
    ///   • Classification: AnomalyType + Severity (for icons, colors, filtering)
    ///   • Context: Buyer/Order/Type/Style/Item (what entity is affected)
    ///   • Anomaly Data: Expected/Actual/Deviation (the numbers)
    ///   • Human Text: Description + RecommendedAction (AI-generated explanations)
    ///   • Status: Current state + who acted + when
    /// </summary>
    public class AnomalyAlertAPIModel
    {
        // ── Identity ────────────────────────────────────────────
        public Guid AlertId { get; set; }

        // ── Classification ──────────────────────────────────────
        // 🎓 AnomalyType: "OVER_CONSUMPTION", "PRICE_SPIKE", or "WASTE_DAMAGE"
        public string AnomalyType { get; set; } = string.Empty;

        // 🎓 Severity: "CRITICAL", "HIGH", "MEDIUM", or "LOW"
        public string Severity { get; set; } = string.Empty;

        // ── Entity Context ──────────────────────────────────────
        // 🎓 These identify WHICH style/material triggered the alert.
        //    The frontend uses them to build navigation links.
        public int BuyerCode { get; set; }
        public string Order { get; set; } = string.Empty;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }

        // ── Anomaly Data ────────────────────────────────────────
        // 🎓 The actual numbers — frontend can display these in charts or comparison views.
        //    ExpectedValue: what the system predicted / what's normal
        //    ActualValue: what was observed
        //    DeviationPercentage: how far off (already calculated)
        //    Unit: measurement unit (YDS, KGS) or currency (USD, GBP)
        public decimal ExpectedValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal DeviationPercentage { get; set; }
        public string? Unit { get; set; }

        // ── Human-Readable Text ─────────────────────────────────
        // 🎓 AI-generated descriptions — ready to display directly in the UI.
        //    Description: what happened, with specific numbers
        //    RecommendedAction: what the user should do about it
        public string Description { get; set; } = string.Empty;
        public string? RecommendedAction { get; set; }

        // ── Status & Audit ──────────────────────────────────────
        // 🎓 Status lifecycle: NEW → READ → ACKNOWLEDGED → RESOLVED/DISMISSED
        public string Status { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string? AcknowledgedByUserId { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? UserNote { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SUMMARY MODEL (compact version for the notification dropdown)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Compact alert for the bell icon dropdown list.
    /// Shows just enough to understand and triage — click for full details.
    ///
    /// 🎓 WHY A SEPARATE MODEL?
    /// The full model has 20+ fields. The notification dropdown only needs
    /// type, severity, a one-line description, and when it happened.
    /// Sending less data = faster rendering of the dropdown.
    /// </summary>
    public class AnomalyAlertSummaryAPIModel
    {
        public Guid AlertId { get; set; }
        public string AnomalyType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;

        // 🎓 Just enough context to identify the style
        public int BuyerCode { get; set; }
        public string Order { get; set; } = string.Empty;
        public string StyleCode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }

        // 🎓 The headline — what the user sees in the dropdown
        public string Description { get; set; } = string.Empty;
        public decimal DeviationPercentage { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UPDATE MODEL (sent by PUT /api/anomaly/alerts/{id}/status)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Status update request — the ONLY write operation users do on alerts.
    ///
    /// 🎓 VALID STATUS TRANSITIONS:
    ///   NEW → READ (system sets this when user views the alert)
    ///   NEW/READ → ACKNOWLEDGED (user clicks "I've seen this")
    ///   ANY → RESOLVED (user confirms the issue is fixed)
    ///   ANY → DISMISSED (user decides it's not a real issue)
    ///
    /// 🎓 WHY NO UserId FIELD?
    /// UserId is extracted from the JWT token (ClaimTypes.NameIdentifier),
    /// following the security protocol: never send identity as a parameter.
    /// </summary>
    public class UpdateAlertStatusAPIModel
    {
        // 🎓 One of: "READ", "ACKNOWLEDGED", "RESOLVED", "DISMISSED"
        public string NewStatus { get; set; } = string.Empty;

        // 🎓 Optional note — why the user is resolving/dismissing this alert.
        //    Useful for audit trail: "False alarm — bulk order was pre-approved"
        public string? UserNote { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SCAN REQUEST MODEL (sent by POST /api/anomaly/scan/style)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Request to trigger an on-demand anomaly scan for a specific style.
    /// Used when a user navigates to a style and wants fresh anomaly data.
    /// </summary>
    public class ScanStyleRequestAPIModel
    {
        public int BuyerCode { get; set; }
        public string Order { get; set; } = string.Empty;
        public int TypeCode { get; set; }
        public string StyleCode { get; set; } = string.Empty;
    }
}
