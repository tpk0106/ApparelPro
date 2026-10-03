using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApparelPro.AI.Abstractions;
using ApparelPro.Data;
using ApparelPro.Data.Models.AI;

namespace ApparelPro.AI.Services;

// 🎓 WHAT: The anomaly detection engine — scans material consumption data
//          and creates AnomalyAlert records when thresholds are breached.
// WHY: Phase 3 of the AI roadmap. Detects three types of anomalies:
//   1. OVER_CONSUMPTION — actual issued > planned + allowance
//   2. PRICE_SPIKE — unit price significantly above historical average
//   3. WASTE_DAMAGE — damaged/returned quantities exceed normal thresholds
//
// 🎓 ARCHITECTURE:
// - Uses IServiceScopeFactory to create scoped DbContext instances
//   (same pattern as EmbeddingSyncJob — singletons can't inject scoped services)
// - Computes a SHA-256 fingerprint for deduplication — same anomaly won't
//   create multiple alerts across successive scan cycles
// - Reads AnomalyRule thresholds from DB so admins can tune sensitivity

public sealed class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnomalyDetectionService> _logger;

    public AnomalyDetectionService(
        IServiceScopeFactory scopeFactory,
        ILogger<AnomalyDetectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<int> RunFullScanAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Starting full anomaly detection scan...");
        var totalNewAlerts = 0;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 Load all active rules once — avoids repeated DB hits during scan
        var rules = await db.AnomalyRules
            .Where(r => r.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            _logger.LogWarning("No active anomaly rules found. Skipping scan.");
            return 0;
        }

        // 🎓 Get all distinct style keys that have material cost profiles
        //    (no point scanning styles with no materials)
        var styleKeys = await db.StyleMaterialCostProfiles
            .Select(p => new { p.BuyerCode, p.Order, p.TypeCode, p.StyleCode })
            .Distinct()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} styles with materials to scan", styleKeys.Count);

        foreach (var key in styleKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var alerts = await DetectAnomaliesForStyleAsync(
                db, rules, key.BuyerCode, key.Order, key.TypeCode, key.StyleCode,
                cancellationToken);

            totalNewAlerts += alerts;
        }

        _logger.LogInformation(
            "✅ Full scan complete. {NewAlerts} new alerts created across {StyleCount} styles.",
            totalNewAlerts, styleKeys.Count);

        return totalNewAlerts;
    }

    /// <inheritdoc />
    public async Task<int> ScanStyleAsync(
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        var rules = await db.AnomalyRules
            .Where(r => r.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return await DetectAnomaliesForStyleAsync(
            db, rules, buyerCode, order, typeCode, styleCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AnomalyAlert>> GetActiveAlertsForStyleAsync(
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 "Active" means not RESOLVED or DISMISSED — the user hasn't dealt with it yet
        return await db.AnomalyAlerts
            .Where(a =>
                a.BuyerCode == buyerCode &&
                a.Order == order &&
                a.TypeCode == typeCode &&
                a.StyleCode == styleCode &&
                a.Status != "RESOLVED" &&
                a.Status != "DISMISSED")
            .OrderByDescending(a => a.DetectedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<AnomalyAlert> Alerts, int TotalCount)> GetActiveAlertsAsync(
        int pageNumber, int pageSize,
        string? anomalyTypeFilter = null,
        string? severityFilter = null,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 STATUS FILTER LOGIC:
        // When NO statusFilter is provided (default panel view), exclude RESOLVED
        // and DISMISSED — the user wants to see actionable alerts only.
        // When statusFilter IS provided (user picked a specific status from the
        // dropdown), show ONLY alerts matching that exact status. This lets
        // merchandisers review historical resolved/dismissed alerts too.
        IQueryable<AnomalyAlert> query;

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            // 🎓 Explicit status filter — show exactly what the user asked for,
            //    even if it's RESOLVED or DISMISSED
            query = db.AnomalyAlerts
                .Where(a => a.Status == statusFilter.ToUpperInvariant())
                .AsNoTracking();
        }
        else
        {
            // 🎓 No status filter — default to "actionable only" (exclude closed alerts)
            query = db.AnomalyAlerts
                .Where(a => a.Status != "RESOLVED" && a.Status != "DISMISSED")
                .AsNoTracking();
        }

        // 🎓 Optional filters for the notification panel dropdowns
        if (!string.IsNullOrWhiteSpace(anomalyTypeFilter))
            query = query.Where(a => a.AnomalyType == anomalyTypeFilter);

        if (!string.IsNullOrWhiteSpace(severityFilter))
            query = query.Where(a => a.Severity == severityFilter);

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .OrderByDescending(a => a.DetectedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (alerts, totalCount);
    }

    /// <inheritdoc />
    public async Task<List<AnomalyAlert>> GetRecentAlertsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 Recent alerts for the bell dropdown — compact summaries.
        // Returns the latest N alerts regardless of status, ordered newest first.
        // The dropdown shows what just happened, even if the user already read them.
        return await db.AnomalyAlerts
            .OrderByDescending(a => a.DetectedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 Only NEW status counts as "unread" for the badge
        return await db.AnomalyAlerts
            .Where(a => a.Status == "NEW")
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAlertStatusAsync(
        Guid alertId, string newStatus, string userId, string? userNote = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

        // 🎓 ExecuteUpdateAsync — single round-trip, no tracking overhead.
        //    Same pattern used in the stock management services.
        var affected = await db.AnomalyAlerts
            .Where(a => a.AlertId == alertId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Status, newStatus)
                .SetProperty(a => a.AcknowledgedByUserId, userId)
                .SetProperty(a => a.AcknowledgedAt, DateTime.UtcNow)
                .SetProperty(a => a.UserNote, userNote)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

        if (affected > 0)
            _logger.LogInformation("Alert {AlertId} → {Status} by {UserId}", alertId, newStatus, userId);

        return affected > 0;
    }

    // ═══════════════════════════════════════════════════════════
    // DETECTION ENGINE — THE CORE LOGIC
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 🎓 Runs all three anomaly checks for a single style and persists any new alerts.
    /// This is the heart of the anomaly detection system.
    /// </summary>
    private async Task<int> DetectAnomaliesForStyleAsync(
        ApparelProDbContext db,
        List<AnomalyRule> rules,
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken)
    {
        var newAlerts = new List<AnomalyAlert>();

        // ─── CHECK 1: Over-Consumption ───────────────────────
        var overConsumptionRule = FindMatchingRule(rules, "OVER_CONSUMPTION", buyerCode);
        if (overConsumptionRule != null)
        {
            var alerts = await CheckOverConsumptionAsync(
                db, overConsumptionRule, buyerCode, order, typeCode, styleCode, cancellationToken);
            newAlerts.AddRange(alerts);
        }

        // ─── CHECK 2: Price Spikes ───────────────────────────
        var priceSpikeRule = FindMatchingRule(rules, "PRICE_SPIKE", buyerCode);
        if (priceSpikeRule != null)
        {
            var alerts = await CheckPriceSpikesAsync(
                db, priceSpikeRule, buyerCode, order, typeCode, styleCode, cancellationToken);
            newAlerts.AddRange(alerts);
        }

        // ─── CHECK 3: Waste / Damage ─────────────────────────
        var wasteDamageRule = FindMatchingRule(rules, "WASTE_DAMAGE", buyerCode);
        if (wasteDamageRule != null)
        {
            var alerts = await CheckWasteDamageAsync(
                db, wasteDamageRule, buyerCode, order, typeCode, styleCode, cancellationToken);
            newAlerts.AddRange(alerts);
        }

        if (newAlerts.Count == 0)
            return 0;

        // 🎓 DEDUPLICATION: Check fingerprints against existing active alerts.
        //    If an alert with the same fingerprint already exists and is not
        //    RESOLVED or DISMISSED, skip it — the issue is already flagged.
        var fingerprints = newAlerts.Select(a => a.Fingerprint).ToList();
        var existingFingerprints = await db.AnomalyAlerts
            .Where(a =>
                fingerprints.Contains(a.Fingerprint) &&
                a.Status != "RESOLVED" &&
                a.Status != "DISMISSED")
            .Select(a => a.Fingerprint)
            .ToListAsync(cancellationToken);

        var genuinelyNew = newAlerts
            .Where(a => !existingFingerprints.Contains(a.Fingerprint))
            .ToList();

        if (genuinelyNew.Count > 0)
        {
            db.AnomalyAlerts.AddRange(genuinelyNew);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Style {Buyer}/{Order}/{Type}/{Style}: {New} new alerts (skipped {Dup} duplicates)",
                buyerCode, order, typeCode, styleCode,
                genuinelyNew.Count, newAlerts.Count - genuinelyNew.Count);
        }

        return genuinelyNew.Count;
    }

    // ─── CHECK 1: OVER-CONSUMPTION ──────────────────────────

    /// <summary>
    /// 🎓 OVER-CONSUMPTION DETECTION LOGIC:
    /// For each material in the style's cost profile, compare:
    ///   - PLANNED: StyleMaterialCostProfile.TotalConsumption (the BOM says we need this much)
    ///   - ACTUAL:  OrderwiseStockMaster.IssuedQuantity (we actually issued this much from stock)
    ///   - ALLOWANCE: Average PercentageAllowance from the consumption ledger entries
    ///
    /// Expected = Planned × (1 + Allowance/100)
    /// Deviation = ((Actual - Expected) / Expected) × 100
    ///
    /// If Deviation >= LowThreshold → create an alert at the appropriate severity.
    /// </summary>
    private async Task<List<AnomalyAlert>> CheckOverConsumptionAsync(
        ApparelProDbContext db, AnomalyRule rule,
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken)
    {
        var alerts = new List<AnomalyAlert>();

        // 🎓 Get all materials for this style from the cost profile
        var materials = await db.StyleMaterialCostProfiles
            .Where(p =>
                p.BuyerCode == buyerCode &&
                p.Order == order &&
                p.TypeCode == typeCode &&
                p.StyleCode == styleCode &&
                p.TotalConsumption > 0)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (materials.Count == 0) return alerts;

        // 🎓 Get average allowance percentages from the consumption ledger
        //    (grouped by ItemCode since the ledger has per-color/size rows)
        var allowances = await db.StyleMaterialConsumptionLedgers
            .Where(l =>
                l.BuyerCode == buyerCode &&
                l.Order == order &&
                l.TypeCode == typeCode &&
                l.StyleCode == styleCode)
            .GroupBy(l => l.StockCode + l.ItemCode + l.Feature1 + l.Feature2 + l.Feature3 + l.Feature4)
            .Select(g => new
            {
                // 🎓 Reconstruct the 22-char composite to match CostProfile.ItemCode
                ItemCode = g.Key,
                AvgAllowance = g.Average(l => l.PercentageAllowance)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allowanceMap = allowances.ToDictionary(a => a.ItemCode, a => a.AvgAllowance);

        // 🎓 Get actual issued quantities from the stock master
        var stockMasters = await db.OrderwiseStockMasters
            .Where(s =>
                s.BuyerCode == buyerCode &&
                s.Order == order)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var issuedMap = stockMasters.ToDictionary(s => s.ItemCode, s => s.IssuedQuantity);

        foreach (var material in materials)
        {
            // 🎓 Skip if stock code filter doesn't match
            if (!string.IsNullOrWhiteSpace(rule.StockCodeFilter) &&
                !material.ItemCode.StartsWith(rule.StockCodeFilter))
                continue;

            var planned = material.TotalConsumption;
            var allowancePct = allowanceMap.GetValueOrDefault(material.ItemCode, 0m);
            var expected = planned * (1 + allowancePct / 100m);

            // 🎓 No point checking if nothing has been issued yet
            if (!issuedMap.TryGetValue(material.ItemCode, out var actualIssued) || actualIssued <= 0)
                continue;

            // 🎓 Skip if within allowance — nothing anomalous
            if (expected <= 0 || actualIssued <= expected)
                continue;

            var deviation = ((actualIssued - expected) / expected) * 100m;
            var severity = ClassifySeverity(deviation, rule);

            if (severity == null) continue; // Below minimum threshold

            alerts.Add(new AnomalyAlert
            {
                AnomalyType = "OVER_CONSUMPTION",
                Severity = severity,
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                ItemCode = material.ItemCode,
                ItemDescription = material.Description,
                ExpectedValue = expected,
                ActualValue = actualIssued,
                DeviationPercentage = Math.Round(deviation, 2),
                Unit = material.ItemUnit,
                Description = $"Material \"{material.Description}\" issued {actualIssued:N2} {material.ItemUnit} " +
                              $"against planned {planned:N2} + {allowancePct:N1}% allowance = {expected:N2} {material.ItemUnit}. " +
                              $"Over by {deviation:N1}%.",
                RecommendedAction = severity switch
                {
                    "CRITICAL" => "URGENT: Investigate immediately. Check cutting room waste reports. " +
                                  "Review marker efficiency. Consider halting further issues until root cause is identified.",
                    "HIGH" => "Review cutting and issue records. Check if marker was updated after consumption was planned. " +
                              "Verify if returns were processed correctly.",
                    "MEDIUM" => "Monitor this material. Review issue patterns — could be gradual over-cutting or " +
                                "mis-sized markers.",
                    _ => "Note for review. May be within acceptable operational variance."
                },
                Status = "NEW",
                DetectedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Fingerprint = ComputeFingerprint("OVER_CONSUMPTION", buyerCode, order, typeCode, styleCode, material.ItemCode)
            });
        }

        return alerts;
    }

    // ─── CHECK 2: PRICE SPIKES ──────────────────────────────

    /// <summary>
    /// 🎓 PRICE SPIKE DETECTION LOGIC:
    /// Compare each material's UnitPrice in this style against the historical
    /// average UnitPrice for the SAME ItemCode across ALL styles.
    ///
    /// Historical Average = AVG(UnitPrice) WHERE ItemCode = X AND UnitPrice > 0
    /// Deviation = ((CurrentPrice - HistoricalAvg) / HistoricalAvg) × 100
    ///
    /// A spike means someone is paying significantly more for the same material
    /// than they historically have — could indicate supplier price increase,
    /// currency fluctuation, or data entry error.
    /// </summary>
    private async Task<List<AnomalyAlert>> CheckPriceSpikesAsync(
        ApparelProDbContext db, AnomalyRule rule,
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken)
    {
        var alerts = new List<AnomalyAlert>();

        // 🎓 Get materials for this style
        var materials = await db.StyleMaterialCostProfiles
            .Where(p =>
                p.BuyerCode == buyerCode &&
                p.Order == order &&
                p.TypeCode == typeCode &&
                p.StyleCode == styleCode &&
                p.UnitPrice > 0)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (materials.Count == 0) return alerts;

        // 🎓 Get historical average prices per ItemCode across all styles
        var itemCodes = materials.Select(m => m.ItemCode).Distinct().ToList();
        var historicalAvgs = await db.StyleMaterialCostProfiles
            .Where(p => itemCodes.Contains(p.ItemCode) && p.UnitPrice > 0)
            .GroupBy(p => p.ItemCode)
            .Select(g => new
            {
                ItemCode = g.Key,
                AvgPrice = g.Average(p => p.UnitPrice),
                Count = g.Count()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 🎓 Need at least 2 data points for a meaningful comparison
        var avgMap = historicalAvgs
            .Where(h => h.Count >= 2)
            .ToDictionary(h => h.ItemCode, h => h.AvgPrice);

        foreach (var material in materials)
        {
            if (!string.IsNullOrWhiteSpace(rule.StockCodeFilter) &&
                !material.ItemCode.StartsWith(rule.StockCodeFilter))
                continue;

            if (!avgMap.TryGetValue(material.ItemCode, out var avgPrice) || avgPrice <= 0)
                continue;

            var deviation = ((material.UnitPrice - avgPrice) / avgPrice) * 100m;

            // 🎓 Only flag price INCREASES, not decreases (a lower price is good!)
            if (deviation <= 0) continue;

            var severity = ClassifySeverity(deviation, rule);
            if (severity == null) continue;

            alerts.Add(new AnomalyAlert
            {
                AnomalyType = "PRICE_SPIKE",
                Severity = severity,
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                ItemCode = material.ItemCode,
                ItemDescription = material.Description,
                ExpectedValue = avgPrice,
                ActualValue = material.UnitPrice,
                DeviationPercentage = Math.Round(deviation, 2),
                Unit = material.Currency,
                Description = $"Material \"{material.Description}\" priced at {material.UnitPrice:N4} {material.Currency} — " +
                              $"historical average is {avgPrice:N4} {material.Currency}. " +
                              $"{deviation:N1}% above average.",
                RecommendedAction = severity switch
                {
                    "CRITICAL" => "URGENT: Verify this price with the supplier. Check for data entry errors. " +
                                  "Compare with alternative suppliers.",
                    "HIGH" => "Review supplier quotation. Check if currency exchange rates changed. " +
                              "Consider re-negotiating or finding alternative sources.",
                    "MEDIUM" => "Monitor pricing trend. May indicate gradual supplier price increase. " +
                                "Flag for next buyer negotiation.",
                    _ => "Minor price increase noted. Review during next procurement cycle."
                },
                Status = "NEW",
                DetectedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Fingerprint = ComputeFingerprint("PRICE_SPIKE", buyerCode, order, typeCode, styleCode, material.ItemCode)
            });
        }

        return alerts;
    }

    // ─── CHECK 3: WASTE / DAMAGE ────────────────────────────

    /// <summary>
    /// 🎓 WASTE/DAMAGE DETECTION LOGIC:
    /// Compare damaged + supplier-returned quantities against total ordered:
    ///
    /// WasteRatio = (DamagedQuantity + SupplierReturnQuantity) / OrderedQuantity × 100
    ///
    /// High waste ratios indicate quality problems with the supplier's materials
    /// or issues in the factory's handling/storage.
    /// </summary>
    private async Task<List<AnomalyAlert>> CheckWasteDamageAsync(
        ApparelProDbContext db, AnomalyRule rule,
        int buyerCode, string order, int typeCode, string styleCode,
        CancellationToken cancellationToken)
    {
        var alerts = new List<AnomalyAlert>();

        // 🎓 Get stock master records for this order
        //    (StockMaster is keyed by BuyerCode + Order + ItemCode, not by Style,
        //     so we get all items for the order and cross-reference with the style's materials)
        var stockMasters = await db.OrderwiseStockMasters
            .Where(s =>
                s.BuyerCode == buyerCode &&
                s.Order == order &&
                s.OrderedQuantity > 0)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (stockMasters.Count == 0) return alerts;

        // 🎓 Cross-reference with this style's materials to only flag relevant items
        var styleMaterialCodes = await db.StyleMaterialCostProfiles
            .Where(p =>
                p.BuyerCode == buyerCode &&
                p.Order == order &&
                p.TypeCode == typeCode &&
                p.StyleCode == styleCode)
            .Select(p => new { p.ItemCode, p.Description })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var descriptionMap = styleMaterialCodes.ToDictionary(m => m.ItemCode, m => m.Description);

        foreach (var stock in stockMasters)
        {
            // 🎓 Only check materials that belong to this style
            if (!descriptionMap.ContainsKey(stock.ItemCode))
                continue;

            if (!string.IsNullOrWhiteSpace(rule.StockCodeFilter) &&
                !stock.ItemCode.StartsWith(rule.StockCodeFilter))
                continue;

            var wasteQty = stock.DamagedQuantity + stock.SupplierReturnQuantity;
            if (wasteQty <= 0) continue;

            var wastePercentage = (wasteQty / stock.OrderedQuantity) * 100m;
            var severity = ClassifySeverity(wastePercentage, rule);
            if (severity == null) continue;

            var description = descriptionMap[stock.ItemCode];

            alerts.Add(new AnomalyAlert
            {
                AnomalyType = "WASTE_DAMAGE",
                Severity = severity,
                BuyerCode = buyerCode,
                Order = order,
                TypeCode = typeCode,
                StyleCode = styleCode,
                ItemCode = stock.ItemCode,
                ItemDescription = description,
                ExpectedValue = 0, // 🎓 Ideal waste is zero
                ActualValue = wasteQty,
                DeviationPercentage = Math.Round(wastePercentage, 2),
                Unit = stock.Unit,
                Description = $"Material \"{description}\": {stock.DamagedQuantity:N2} {stock.Unit} damaged + " +
                              $"{stock.SupplierReturnQuantity:N2} {stock.Unit} returned to supplier = " +
                              $"{wasteQty:N2} {stock.Unit} total waste ({wastePercentage:N1}% of {stock.OrderedQuantity:N2} ordered).",
                RecommendedAction = severity switch
                {
                    "CRITICAL" => "URGENT: Quality crisis. Inspect remaining stock immediately. " +
                                  "Contact supplier about defective batch. Consider claim for damages.",
                    "HIGH" => "Review damage reports. Check storage conditions. " +
                              "Discuss quality improvement with supplier.",
                    "MEDIUM" => "Elevated waste level. Review handling procedures. " +
                                "Check if specific colors/sizes are more affected.",
                    _ => "Minor waste noted. Normal operational variance possible."
                },
                Status = "NEW",
                DetectedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Fingerprint = ComputeFingerprint("WASTE_DAMAGE", buyerCode, order, typeCode, styleCode, stock.ItemCode)
            });
        }

        return alerts;
    }

    // ═══════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 🎓 SEVERITY CLASSIFICATION:
    /// Maps a deviation percentage to a severity level using the rule's thresholds.
    /// Returns null if the deviation is below the minimum (LowThreshold).
    /// </summary>
    private static string? ClassifySeverity(decimal deviation, AnomalyRule rule)
    {
        if (deviation >= rule.CriticalThreshold) return "CRITICAL";
        if (deviation >= rule.HighThreshold) return "HIGH";
        if (deviation >= rule.MediumThreshold) return "MEDIUM";
        if (deviation >= rule.LowThreshold) return "LOW";
        return null; // Below threshold — no alert
    }

    /// <summary>
    /// 🎓 RULE MATCHING:
    /// Finds the most specific matching rule for an anomaly type.
    /// Priority: buyer-specific rule > generic rule (no buyer filter).
    /// </summary>
    private static AnomalyRule? FindMatchingRule(
        List<AnomalyRule> rules, string anomalyType, int buyerCode)
    {
        // 🎓 Prefer a buyer-specific rule if one exists
        var buyerRule = rules.FirstOrDefault(r =>
            r.AnomalyType == anomalyType && r.BuyerCode == buyerCode);
        if (buyerRule != null) return buyerRule;

        // 🎓 Fall back to the generic rule (no buyer filter)
        return rules.FirstOrDefault(r =>
            r.AnomalyType == anomalyType && r.BuyerCode == null);
    }

    /// <summary>
    /// 🎓 FINGERPRINT for deduplication:
    /// SHA-256 hash of (AnomalyType|BuyerCode|Order|TypeCode|StyleCode|ItemCode).
    /// Two scan runs that find the same issue produce the same fingerprint,
    /// so the second run won't create a duplicate alert.
    /// </summary>
    private static string ComputeFingerprint(
        string anomalyType, int buyerCode, string order,
        int typeCode, string styleCode, string itemCode)
    {
        var input = $"{anomalyType}|{buyerCode}|{order}|{typeCode}|{styleCode}|{itemCode}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
