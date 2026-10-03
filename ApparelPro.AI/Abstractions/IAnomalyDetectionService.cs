namespace ApparelPro.AI.Abstractions;

// 🎓 WHAT: Interface for the anomaly detection engine.
// WHY: Dependency Inversion Principle — controllers and background jobs
//      depend on this abstraction, never the concrete AnomalyDetectionService.
//      This also enables unit testing with mock implementations.

/// <summary>
/// Scans material consumption data for anomalies (over-consumption, price spikes,
/// waste/damage) and creates AnomalyAlert records in the database.
/// </summary>
public interface IAnomalyDetectionService
{
    /// <summary>
    /// 🎓 FULL SCAN: Runs all three anomaly checks across all active styles.
    /// Called by the AnomalyDetectionJob background service on its timer cycle.
    /// Returns the number of NEW alerts created (excludes duplicates).
    /// </summary>
    Task<int> RunFullScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 TARGETED SCAN: Runs anomaly checks for a specific style only.
    /// Called when a user opens a style and we want fresh anomaly data.
    /// Faster than a full scan — useful for on-demand checks.
    /// </summary>
    Task<int> ScanStyleAsync(
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 GET ACTIVE ALERTS: Retrieves unresolved anomaly alerts for a style.
    /// Used by AI chat context injection — when the user asks about a style,
    /// we inject these alerts into the prompt so Claude can mention them.
    /// </summary>
    Task<List<Data.Models.AI.AnomalyAlert>> GetActiveAlertsForStyleAsync(
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 GET ALL ALERTS (PAGINATED): Retrieves alerts for the notification panel.
    /// Supports filtering by anomaly type, severity, AND status.
    ///
    /// 🎓 STATUS FILTER BEHAVIOUR:
    /// When statusFilter is null (default), the query excludes RESOLVED and DISMISSED
    /// alerts — showing only actionable ones (NEW, READ, ACKNOWLEDGED).
    /// When statusFilter is provided, returns ONLY alerts matching that exact status,
    /// so merchandisers can review historical resolved/dismissed alerts too.
    /// </summary>
    Task<(List<Data.Models.AI.AnomalyAlert> Alerts, int TotalCount)> GetActiveAlertsAsync(
        int pageNumber, int pageSize,
        string? anomalyTypeFilter = null,
        string? severityFilter = null,
        string? statusFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 RECENT ALERTS: Returns the latest N alerts for the bell dropdown.
    /// Ordered by DetectedAt descending, regardless of status — the dropdown
    /// shows what just happened, even if the user already acknowledged them.
    /// </summary>
    Task<List<Data.Models.AI.AnomalyAlert>> GetRecentAlertsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 UNREAD COUNT: Fast count for the bell icon badge number.
    /// Only counts NEW status alerts.
    /// </summary>
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 🎓 UPDATE STATUS: Acknowledge, resolve, or dismiss an alert.
    /// Records who acted and when, plus an optional user note.
    /// </summary>
    Task<bool> UpdateAlertStatusAsync(
        Guid alertId, string newStatus, string userId, string? userNote = null,
        CancellationToken cancellationToken = default);
}
