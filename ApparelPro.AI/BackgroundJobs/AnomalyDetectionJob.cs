using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;

namespace ApparelPro.AI.BackgroundJobs;

/// <summary>
/// Background service that periodically scans material consumption data
/// for anomalies (over-consumption, price spikes, waste/damage).
///
/// 🎓 HOW THIS FITS IN PHASE 3:
/// The anomaly detection pipeline has three parts:
///   1. AnomalyDetectionService — the engine (scans data, creates AnomalyAlert rows)
///   2. AnomalyDetectionJob — the scheduler (THIS class — runs the engine periodically)
///   3. AnomalyAlertController — the API (serves alerts to the React front-end)
///
/// Without this job, anomalies would only be detected when someone explicitly
/// triggers a scan via the API. With it, the system proactively surfaces issues
/// — a merchandiser opens their dashboard and sees "⚠️ 3 new anomaly alerts"
/// without having to ask.
///
/// 🎓 PATTERN: Same as EmbeddingSyncJob
/// Both are BackgroundService subclasses that:
///   - Wait on startup (let the app fully initialise)
///   - Run a periodic task on a configurable interval
///   - Catch and log all exceptions (never crash the host)
///   - Use IServiceScopeFactory internally (via the service they call)
///
/// 🎓 KEY DIFFERENCE FROM EmbeddingSyncJob:
/// EmbeddingSyncJob resolves scoped services from IServiceScopeFactory directly.
/// AnomalyDetectionJob injects IAnomalyDetectionService (a singleton) which
/// manages its own scopes internally — so this job is simpler. It just calls
/// RunFullScanAsync() and lets the service handle the DbContext lifecycle.
///
/// 🎓 WHY A SEPARATE JOB INSTEAD OF ADDING TO EmbeddingSyncJob?
/// Single Responsibility Principle (SRP):
///   - EmbeddingSyncJob syncs data INTO the vector store (Qdrant)
///   - AnomalyDetectionJob scans data for threshold breaches (SQL Server)
/// They run on different intervals, fail independently, and produce
/// different outputs. Combining them would create a monolithic job where
/// a Qdrant outage blocks anomaly scanning, which makes no sense.
/// </summary>
public sealed class AnomalyDetectionJob : BackgroundService
{
    // ── Dependencies ─────────────────────────────────────

    private readonly IAnomalyDetectionService _anomalyDetectionService;
    private readonly AiSettings _settings;
    private readonly ILogger<AnomalyDetectionJob> _logger;

    /// <summary>
    /// 🎓 CONSTRUCTOR:
    /// We inject IAnomalyDetectionService directly (it's a singleton).
    /// No need for IServiceScopeFactory here — the detection service
    /// manages its own scoped DbContext instances internally.
    ///
    /// IOptions&lt;AiSettings&gt; provides the scan interval from appsettings.json:
    ///   "AiSettings": { "AnomalyScanIntervalMinutes": 30 }
    /// </summary>
    public AnomalyDetectionJob(
        IAnomalyDetectionService anomalyDetectionService,
        IOptions<AiSettings> settings,
        ILogger<AnomalyDetectionJob> logger)
    {
        _anomalyDetectionService = anomalyDetectionService;
        _settings = settings.Value;
        _logger = logger;
    }

    // ── Main execution loop ──────────────────────────

    /// <summary>
    /// 🎓 THE HEART OF THE BACKGROUND SERVICE:
    /// ExecuteAsync is called once when the application starts.
    /// It runs until the stoppingToken is cancelled (app shutdown).
    ///
    /// Our implementation:
    ///   1. Wait 60 seconds on startup (let the app, DB, and other services settle)
    ///   2. Run the scan loop:
    ///      a. Call AnomalyDetectionService.RunFullScanAsync()
    ///      b. Wait for AnomalyScanIntervalMinutes (default 30 min)
    ///      c. Repeat
    ///
    /// 🎓 WHY 60-SECOND STARTUP DELAY (vs. EmbeddingSyncJob's 30s)?
    /// Anomaly detection reads from the same tables that the embedding sync
    /// might be loading. A slightly longer delay ensures:
    ///   - EF Core migrations have completed
    ///   - The AnomalyRule seed data from HasData() is available
    ///   - Other background services have had time to initialise
    ///   - No contention with EmbeddingSyncJob's first cycle
    ///
    /// 🎓 ERROR HANDLING:
    /// Same resilience pattern as EmbeddingSyncJob:
    ///   - Log the error, wait for the next cycle, try again
    ///   - NEVER let an exception kill the background service
    ///   - A crashed scan job means anomalies go undetected silently —
    ///     much worse than a temporary scan failure
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "AnomalyDetectionJob starting. Scan interval: {Interval} minutes",
            _settings.AnomalyScanIntervalMinutes);

        // ── Startup delay ────────────────────────────
        // 🎓 Give the application time to fully initialise.
        // AnomalyRule seed data (HasData) needs to be available before
        // the first scan, or the severity classification will fail.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return; // App is shutting down during startup delay
        }

        _logger.LogInformation("AnomalyDetectionJob startup delay complete. Entering scan loop.");

        // ── Scan loop ────────────────────────────────
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunScanCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // Clean shutdown
            }
            catch (Exception ex)
            {
                // 🎓 RESILIENCE: Log and continue.
                // The next cycle might succeed if the issue was transient
                // (DB connection blip, temporary lock contention, etc.).
                // We don't rethrow — a crashed job is invisible to users.
                _logger.LogError(ex,
                    "AnomalyDetectionJob scan cycle failed. Will retry in {Interval} minutes.",
                    _settings.AnomalyScanIntervalMinutes);
            }

            // ── Wait for next cycle ──────────────────
            try
            {
                await Task.Delay(
                    TimeSpan.FromMinutes(_settings.AnomalyScanIntervalMinutes),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // App is shutting down
            }
        }

        _logger.LogInformation("AnomalyDetectionJob stopping.");
    }

    // ── Scan cycle ───────────────────────────────────────

    /// <summary>
    /// 🎓 ONE SCAN CYCLE:
    /// Delegates all the heavy lifting to AnomalyDetectionService.RunFullScanAsync().
    ///
    /// The service internally:
    ///   1. Creates a scoped DbContext (via IServiceScopeFactory)
    ///   2. Loads all active AnomalyRules (configurable thresholds)
    ///   3. Queries all active styles with their material consumption ledgers
    ///   4. Runs three detection checks per style:
    ///      a. Over-consumption (actual > planned + wastage allowance)
    ///      b. Price spike (unit price > historical average × threshold)
    ///      c. Waste/damage (damaged qty > normal threshold)
    ///   5. Deduplicates using SHA-256 fingerprints (no duplicate alerts)
    ///   6. Persists new AnomalyAlert rows to SQL Server
    ///
    /// 🎓 WHY IS THIS METHOD SO SIMPLE?
    /// Single Responsibility: The job's ONLY job is scheduling.
    /// All detection logic lives in AnomalyDetectionService.
    /// This makes both classes easier to test:
    ///   - Test the service in isolation (pass mock data, verify alerts)
    ///   - Test the job in isolation (verify timing, error handling)
    /// </summary>
    private async Task RunScanCycleAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting anomaly detection scan cycle...");

        var alertsCreated = await _anomalyDetectionService
            .RunFullScanAsync(stoppingToken);

        if (alertsCreated > 0)
        {
            // 🎓 LOG AT WARNING LEVEL when new alerts are created.
            // This makes anomaly events stand out in structured logging tools
            // (e.g., Seq, Application Insights) without requiring a separate
            // monitoring alert on the log stream. Ops teams typically filter
            // on Warning+ to catch actionable events.
            _logger.LogWarning(
                "Anomaly scan complete. {AlertCount} new alert(s) created. " +
                "Review via GET /api/anomaly/alerts or the notification bell.",
                alertsCreated);
        }
        else
        {
            _logger.LogInformation(
                "Anomaly scan complete. No new anomalies detected.");
        }
    }
}
