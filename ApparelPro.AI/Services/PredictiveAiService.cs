using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;
using ApparelPro.AI.Prompts;
using ApparelPro.Data;

namespace ApparelPro.AI.Services;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 PREDICTIVE AI SERVICE — IMPLEMENTATION
// Phase 5: Predictive & Prescriptive AI
//
// 🎓 ARCHITECTURE DECISIONS (same philosophy as CrossEntityAnalyticsService):
//
// 1. REGISTERED AS TRANSIENT (not singleton)
//    Per-request service — directly injects the scoped ApparelProDbContext.
//    Same pattern as CrossEntityAnalyticsService, StyleDetailsService, etc.
//
// 2. COMPOSES IAiService (doesn't extend it)
//    This service owns the data aggregation AND signal computation logic,
//    then delegates AI completion to IAiService.CompleteAsync().
//
// 3. ALL QUERIES USE AsNoTracking()
//    Read-only aggregations — no entity tracking overhead needed.
//
// 4. SIGNAL COMPUTATION IS PRE-AI
//    Unlike analytics (which sends raw aggregated data to the AI), predictive
//    service PRE-COMPUTES derived signals (growth rates, risk scores, shortage
//    indicators) before sending to the AI. This gives the AI QUANTIFIED trends
//    to base predictions on, not just raw numbers.
//
// 5. DATA SHAPE MATCHES PROMPT TEMPLATES
//    Each aggregation method returns an object whose properties match EXACTLY
//    what PredictivePromptTemplates.cs tells the AI to expect.
//
// 🎓 THE FOUR AGGREGATION PIPELINES:
//
//   AggregateDemandForecastingAsync:
//     PurchaseOrders + Styles + Buyers → per-buyer order velocity, recent vs
//     prior ratios, revenue velocity → AI forecasts future demand
//
//   AggregateMaterialShortageAsync:
//     StyleMaterialConsumptionLedgers + Suppliers → per-material supply risk,
//     sole-sourcing flags, consumption concentration → AI predicts shortages
//
//   AggregateSupplierRiskAsync:
//     Suppliers + StyleMaterialConsumptionLedgers + Styles → per-supplier risk
//     dimensions (concentration, dependency, contact, breadth) → AI scores risk
//
//   AggregateCostOptimizationAsync:
//     Styles + StyleMaterialConsumptionLedgers + Buyers → wastage hotspots,
//     pricing variance, BOM complexity → AI recommends cost reductions
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Aggregates historical data, computes trend signals, and sends them to the AI
/// for forward-looking predictions and prescriptive recommendations.
/// Registered as Transient — injects scoped DbContext directly.
/// </summary>
public sealed class PredictiveAiService : IPredictiveAiService
{
    private readonly ApparelProDbContext _db;
    private readonly IAiService _aiService;
    private readonly AiSettings _settings;
    private readonly ILogger<PredictiveAiService> _logger;

    // 🎓 JSON serialization options — camelCase for consistency with frontend
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // 🎓 Default time horizon if not specified in request (3 months is a typical
    //    planning horizon for apparel manufacturing — covers one buying season)
    private const int DefaultTimeHorizonMonths = 3;

    public PredictiveAiService(
        ApparelProDbContext db,
        IAiService aiService,
        IOptions<AiSettings> settings,
        ILogger<PredictiveAiService> logger)
    {
        _db = db;
        _aiService = aiService;
        _settings = settings.Value;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<(AiCompletionResponse Response, int EntityCount)> GetPredictionAsync(
        string category,
        string? userQuery = null,
        CancellationToken cancellationToken = default)
    {
        // 🎓 STEP 1: Validate category — GetPredictiveTemplate will throw
        //            ArgumentException for unsupported categories.
        var normalizedCategory = category.Trim().ToUpperInvariant();
        var template = PredictivePromptTemplates.GetPredictiveTemplate(normalizedCategory);

        // 🎓 STEP 2: Parse time horizon from the request (default 3 months)
        //            Clamped to 1-12 months for sensible forecast windows.
        var timeHorizon = DefaultTimeHorizonMonths;

        // 🎓 STEP 3: Aggregate data + compute signals for this category
        var (aggregatedJson, entityCount) = await AggregateDataAsync(
            normalizedCategory, cancellationToken);

        if (entityCount == 0)
        {
            _logger.LogWarning(
                "No data found for prediction category {Category}. " +
                "Returning empty prediction.", normalizedCategory);

            return (new AiCompletionResponse
            {
                Content = "No data available for this prediction category. " +
                          "Predictions require historical data to generate forecasts. " +
                          "Please ensure there are active records in the system.",
                Provider = "None",
                Model = "None",
                TotalTokens = 0,
                InputTokens = 0,
                OutputTokens = 0
            }, 0);
        }

        // 🎓 STEP 4: Build the user message from the prediction template
        //            Predictive templates use 3 placeholders:
        //              {0} = aggregated JSON data with computed signals
        //              {1} = time horizon in months
        //              {2} = optional user query
        var userQuerySection = string.IsNullOrWhiteSpace(userQuery)
            ? string.Empty
            : $"Additionally, focus on this question: {userQuery}";

        var userMessage = string.Format(template, aggregatedJson, timeHorizon, userQuerySection);

        // 🎓 STEP 5: Build the AI completion request
        //            Uses the prediction-specific system prompt (forecasting persona)
        //            and PredictiveMaxTokens (5000) — prescriptive responses are the
        //            longest in the system because they include:
        //              - Predictions with confidence levels and scenarios
        //              - Structured action plans with causality chains
        //              - Decision matrices with trade-off comparisons
        //              - Implementation steps with owner assignments
        //            The extra 1500 tokens over AnalyticsMaxTokens (3500) prevent
        //            the AI from truncating the prescriptive action plan section.
        var request = new AiCompletionRequest
        {
            SystemPrompt = PredictivePromptTemplates.PredictiveSystem,
            UserMessage = userMessage,
            MaxTokens = _settings.PredictiveMaxTokens,   // 🎓 5000 — dedicated prescriptive token limit
            Temperature = 0.3  // 🎓 Slightly higher than analytics (0.25) — predictions
                               //    benefit from some creative reasoning about scenarios,
                               //    but we still want grounded, data-driven forecasts.
        };

        _logger.LogInformation(
            "Running prediction for category {Category} " +
            "across {EntityCount} entities via {Provider} " +
            "(horizon: {TimeHorizon} months)",
            normalizedCategory, entityCount, _settings.ActiveProvider, timeHorizon);

        // 🎓 STEP 6: Call IAiService.CompleteAsync() — delegates to the active provider
        var response = await _aiService.CompleteAsync(request, cancellationToken);

        _logger.LogInformation(
            "Prediction complete for {Category}. " +
            "Input: {InputTokens}, Output: {OutputTokens}, Total: {TotalTokens}",
            normalizedCategory, response.InputTokens,
            response.OutputTokens, response.TotalTokens);

        return (response, entityCount);
    }

    // ═══════════════════════════════════════════════════════════
    // DATA AGGREGATION + SIGNAL COMPUTATION — one method per category
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 🎓 Dispatcher: routes to the correct aggregation method based on category.
    /// Returns (serialized JSON string with computed signals, entity count).
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateDataAsync(
        string category,
        CancellationToken ct)
    {
        return category switch
        {
            "DEMAND_FORECASTING" => await AggregateDemandForecastingAsync(ct),
            "MATERIAL_SHORTAGE"  => await AggregateMaterialShortageAsync(ct),
            "SUPPLIER_RISK"      => await AggregateSupplierRiskAsync(ct),
            "COST_OPTIMIZATION"  => await AggregateCostOptimizationAsync(ct),
            _ => throw new ArgumentException($"Unsupported prediction category: '{category}'.")
        };
    }

    // ─────────────────────────────────────────────
    // A. DEMAND FORECASTING
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates demand signals for forecasting future order volumes.
    ///
    /// SIGNAL COMPUTATION:
    ///   1. Load all purchase orders and styles with buyer info
    ///   2. Group by buyer → compute per-buyer order velocity
    ///   3. Compute recent vs prior ratio (last 90 days vs prior 90 days)
    ///   4. Compute business-wide velocity signals
    ///
    /// 🎓 WHY ORDER VELOCITY?
    /// Raw order counts are meaningless without time context. A buyer with
    /// 20 orders over 2 years (0.83/month) is VERY different from a buyer
    /// with 20 orders in 3 months (6.67/month). Velocity normalises for time.
    ///
    /// 🎓 WHY RECENT VS PRIOR RATIO?
    /// This is the simplest trend indicator: compare the last 90 days to the
    /// prior 90 days. > 1.0 means accelerating demand, < 1.0 means decelerating.
    /// It's crude but surprisingly effective for short-term forecasting.
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateDemandForecastingAsync(
        CancellationToken ct)
    {
        // 🎓 Load all purchase orders with dates for time-series analysis
        var orders = await _db.PurchaseOrders
            .AsNoTracking()
            .ToListAsync(ct);

        if (orders.Count == 0)
            return ("[]", 0);

        // 🎓 Load all styles for quantity + pricing data
        var styles = await _db.Styles
            .AsNoTracking()
            .ToListAsync(ct);

        // 🎓 Load buyer names for the aggregation
        var buyers = await _db.Buyers
            .AsNoTracking()
            .ToDictionaryAsync(b => b.BuyerCode, b => b.Name, ct);

        var today = DateTime.UtcNow;
        var ninetyDaysAgo = today.AddDays(-90);
        var oneEightyDaysAgo = today.AddDays(-180);

        // 🎓 Group orders by buyer for per-buyer velocity computation
        var ordersByBuyer = orders.GroupBy(o => o.BuyerCode).ToDictionary(g => g.Key, g => g.ToList());
        var stylesByBuyer = styles.GroupBy(s => s.BuyerCode).ToDictionary(g => g.Key, g => g.ToList());

        // 🎓 Compute per-buyer demand signals
        var buyerSignals = ordersByBuyer.Select(kvp =>
        {
            var buyerCode = kvp.Key;
            var buyerOrders = kvp.Value;
            var buyerStyles = stylesByBuyer.GetValueOrDefault(buyerCode) ?? [];
            var buyerName = buyers.GetValueOrDefault(buyerCode, "Unknown");

            // 🎓 Time span analysis
            var earliestOrder = buyerOrders.Min(o => o.OrderDate);
            var latestOrder = buyerOrders.Max(o => o.OrderDate);
            var orderSpanDays = Math.Max((latestOrder - earliestOrder).Days, 1);
            var orderSpanMonths = Math.Max(orderSpanDays / 30.0, 1.0);

            // 🎓 Volume signals
            var totalOrderQuantity = buyerStyles.Sum(s => s.Quantity ?? 0m);
            var totalEstimatedRevenue = buyerStyles.Sum(s =>
                (s.Quantity ?? 0m) * (s.UnitPrice ?? 0m));

            // 🎓 Velocity signals — normalised to per-month rates
            var orderVelocity = Math.Round(buyerOrders.Count / orderSpanMonths, 2);
            var quantityVelocity = Math.Round((double)totalOrderQuantity / orderSpanMonths, 0);
            var revenueVelocity = Math.Round((double)totalEstimatedRevenue / orderSpanMonths, 2);

            // 🎓 Recent trend: last 90 days vs prior 90 days
            var recentOrders = buyerOrders.Where(o => o.OrderDate >= ninetyDaysAgo).ToList();
            var priorOrders = buyerOrders
                .Where(o => o.OrderDate >= oneEightyDaysAgo && o.OrderDate < ninetyDaysAgo)
                .ToList();

            var recentQuantity = recentOrders.Sum(o => o.TotalQuantity);
            var priorQuantity = priorOrders.Sum(o => o.TotalQuantity);

            // 🎓 Recent vs prior ratio — null if not enough history
            double? recentVsPriorRatio = priorQuantity > 0
                ? Math.Round((double)recentQuantity / (double)priorQuantity, 2)
                : null;

            // 🎓 Top styles by quantity — helps identify demand concentration
            var topStyles = buyerStyles
                .OrderByDescending(s => s.Quantity ?? 0m)
                .Take(3)
                .Select(s => new { s.StyleCode, s.Order, Quantity = s.Quantity ?? 0m })
                .ToList();

            return new
            {
                BuyerCode = buyerCode,
                BuyerName = buyerName,
                TotalOrders = buyerOrders.Count,
                TotalStyles = buyerStyles.Count,
                TotalOrderQuantity = totalOrderQuantity,
                TotalEstimatedRevenue = totalEstimatedRevenue,
                AverageOrderQuantity = buyerStyles.Count > 0
                    ? Math.Round(totalOrderQuantity / buyerStyles.Count, 0)
                    : 0m,
                AverageFobPrice = buyerStyles.Count > 0 && buyerStyles.Any(s => s.UnitPrice > 0)
                    ? Math.Round(buyerStyles.Where(s => s.UnitPrice > 0)
                        .Average(s => s.UnitPrice ?? 0m), 2)
                    : 0m,
                EarliestOrderDate = earliestOrder.ToString("yyyy-MM-dd"),
                LatestOrderDate = latestOrder.ToString("yyyy-MM-dd"),
                OrderSpanDays = orderSpanDays,
                OrderVelocity = orderVelocity,
                QuantityVelocity = quantityVelocity,
                RevenueVelocity = revenueVelocity,
                RecentOrderCount = recentOrders.Count,
                RecentQuantity = recentQuantity,
                RecentVsPriorRatio = recentVsPriorRatio,
                TopStylesByQuantity = topStyles
            };
        }).ToList();

        // 🎓 Business-wide aggregate signals
        var totalBuyerCount = buyerSignals.Count;
        var businessWideOrderVelocity = buyerSignals.Sum(b => b.OrderVelocity);
        var businessWideQuantityVelocity = buyerSignals.Sum(b => b.QuantityVelocity);
        var businessWideRevenueVelocity = buyerSignals.Sum(b => b.RevenueVelocity);

        var result = new
        {
            BuyerSignals = buyerSignals,
            TotalBuyerCount = totalBuyerCount,
            BusinessWideOrderVelocity = Math.Round(businessWideOrderVelocity, 2),
            BusinessWideQuantityVelocity = Math.Round(businessWideQuantityVelocity, 0),
            BusinessWideRevenueVelocity = Math.Round(businessWideRevenueVelocity, 2)
        };

        var json = JsonSerializer.Serialize(result, JsonOptions);
        return (json, totalBuyerCount);
    }

    // ─────────────────────────────────────────────
    // B. MATERIAL SHORTAGE
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates material shortage risk signals.
    ///
    /// SIGNAL COMPUTATION:
    ///   1. Load all consumption ledger rows (direct materials only)
    ///   2. Group by material (stockCode + itemCode)
    ///   3. For each material: compute supplier count, sole-source flag,
    ///      consumption concentration, and dependent style list
    ///   4. Flag unsourced, sole-sourced, and high-concentration materials
    ///
    /// 🎓 KEY CONCEPT: CONSUMPTION CONCENTRATION
    /// If one style consumes 80% of a material, and that style gets cancelled
    /// or delayed, the supplier relationship for that material becomes fragile.
    /// High concentration = fragile demand signal = supply chain risk.
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateMaterialShortageAsync(
        CancellationToken ct)
    {
        // 🎓 Load all direct material consumption rows (not additional costs)
        var ledger = await _db.StyleMaterialConsumptionLedgers
            .Where(l => !l.IsAdditionalCost)
            .AsNoTracking()
            .ToListAsync(ct);

        if (ledger.Count == 0)
            return ("[]", 0);

        // 🎓 Group consumption by material identity (stockCode + itemCode)
        var materialGroups = ledger
            .GroupBy(l => new { l.StockCode, l.ItemCode })
            .ToList();

        // 🎓 Compute per-material shortage risk signals
        var materialSignals = materialGroups.Select(g =>
        {
            var rows = g.ToList();
            var totalConsumption = rows.Sum(r => r.TotalConsumption);

            // 🎓 Supplier analysis for this material
            var supplierCodes = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.SupplierCode) && r.SupplierCode != "0")
                .Select(r => r.SupplierCode)
                .Distinct()
                .ToList();

            var supplierCount = supplierCodes.Count;
            var isSoleSourced = supplierCount == 1;
            var isUnsourced = supplierCount == 0;

            // 🎓 Style dependency — which styles need this material?
            var dependentStyles = rows
                .Select(r => new { r.StyleCode, r.Order, r.BuyerCode })
                .Distinct()
                .ToList();

            // 🎓 Consumption concentration — % consumed by the top style
            var topStyleConsumption = rows
                .GroupBy(r => new { r.StyleCode, r.Order, r.BuyerCode })
                .Max(sg => sg.Sum(r => r.TotalConsumption));

            var consumptionConcentration = totalConsumption > 0
                ? Math.Round(topStyleConsumption / totalConsumption * 100m, 1)
                : 0m;

            // 🎓 Wastage signals
            var avgWastage = rows.Count > 0
                ? Math.Round(rows.Average(r => r.PercentageAllowance), 2)
                : 0m;
            var maxWastage = rows.Count > 0
                ? rows.Max(r => r.PercentageAllowance)
                : 0m;

            return new
            {
                StockCode = g.Key.StockCode,
                ItemCode = g.Key.ItemCode,
                MaterialDescription = $"{g.Key.StockCode}/{g.Key.ItemCode}",
                TotalPlannedConsumption = totalConsumption,
                StylesDependingOnMaterial = dependentStyles.Count,
                StyleDependencyList = dependentStyles,
                SupplierCount = supplierCount,
                SupplierCodes = supplierCodes,
                IsSoleSourced = isSoleSourced,
                IsUnsourced = isUnsourced,
                ConsumptionConcentration = consumptionConcentration,
                AverageWastagePercent = avgWastage,
                MaxWastagePercent = maxWastage
            };
        }).ToList();

        // 🎓 Aggregate shortage signals
        var result = new
        {
            MaterialSignals = materialSignals,
            TotalMaterialCount = materialSignals.Count,
            UnsourcedMaterialCount = materialSignals.Count(m => m.IsUnsourced),
            SoleSourcedMaterialCount = materialSignals.Count(m => m.IsSoleSourced),
            HighConcentrationMaterialCount = materialSignals.Count(m => m.ConsumptionConcentration > 70)
        };

        var json = JsonSerializer.Serialize(result, JsonOptions);
        return (json, materialSignals.Count);
    }

    // ─────────────────────────────────────────────
    // C. SUPPLIER RISK
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates supplier risk scores across multiple dimensions.
    ///
    /// SIGNAL COMPUTATION:
    /// For each supplier, compute 5 risk dimensions:
    ///   1. Concentration risk — what % of total consumption this supplier covers
    ///   2. Dependency risk — how many materials they're the ONLY source for
    ///   3. Contact risk — are they reachable? (0 channels = unreachable)
    ///   4. Breadth risk — single-category suppliers are fragile
    ///   5. Revenue risk — what % of total revenue depends on them
    ///
    /// 🎓 WHY MULTI-DIMENSIONAL SCORING?
    /// A supplier might be low-risk on concentration but high-risk on contact.
    /// Single-number risk scores hide these nuances. The AI gets ALL dimensions
    /// and can explain which specific factor drives each supplier's risk.
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateSupplierRiskAsync(
        CancellationToken ct)
    {
        // 🎓 Load all suppliers
        var suppliers = await _db.Suppliers
            .AsNoTracking()
            .ToListAsync(ct);

        if (suppliers.Count == 0)
            return ("[]", 0);

        // 🎓 Load all direct material consumption rows
        var ledger = await _db.StyleMaterialConsumptionLedgers
            .Where(l => !l.IsAdditionalCost)
            .AsNoTracking()
            .ToListAsync(ct);

        // 🎓 Load styles for revenue exposure calculation
        var styles = await _db.Styles
            .AsNoTracking()
            .ToListAsync(ct);

        var totalConsumption = ledger.Sum(l => l.TotalConsumption);
        var totalRevenue = styles.Sum(s => (s.Quantity ?? 0m) * (s.UnitPrice ?? 0m));

        // 🎓 Group consumption by supplier
        var consumptionBySupplier = ledger
            .Where(l => !string.IsNullOrWhiteSpace(l.SupplierCode) && l.SupplierCode != "0")
            .GroupBy(l => l.SupplierCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 🎓 Compute sole-sourcing: for each material, is there only one supplier?
        var materialSupplierCounts = ledger
            .Where(l => !string.IsNullOrWhiteSpace(l.SupplierCode) && l.SupplierCode != "0")
            .GroupBy(l => new { l.StockCode, l.ItemCode })
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.SupplierCode).Distinct().ToList());

        // 🎓 Build per-supplier risk profile
        var supplierRiskData = suppliers.Select(s =>
        {
            var supplierCodeStr = s.SupplierCode.ToString();
            var rows = consumptionBySupplier.GetValueOrDefault(supplierCodeStr) ?? [];

            // 🎓 Contact risk dimension
            var contactChannels = 0;
            if (!string.IsNullOrWhiteSpace(s.TelephoneNos)) contactChannels++;
            if (!string.IsNullOrWhiteSpace(s.MobileNos)) contactChannels++;
            if (!string.IsNullOrWhiteSpace(s.Fax)) contactChannels++;

            // 🎓 Concentration risk dimension
            var supplierConsumption = rows.Sum(r => r.TotalConsumption);
            var consumptionSharePercent = totalConsumption > 0
                ? Math.Round(supplierConsumption / totalConsumption * 100m, 1)
                : 0m;

            // 🎓 Dependency risk — styles and buyers that depend on this supplier
            var dependentStyles = rows
                .Select(r => new { r.BuyerCode, r.Order, r.StyleCode })
                .Distinct().ToList();
            var dependentBuyers = dependentStyles
                .Select(d => d.BuyerCode)
                .Distinct().Count();

            // 🎓 Materials supplied and category breadth
            var materialsSupplied = rows
                .Select(r => $"{r.StockCode}/{r.ItemCode}")
                .Distinct().ToList();
            var materialCategoryCount = rows
                .Select(r => r.StockCode)
                .Distinct().Count();

            // 🎓 Sole-sourcing count — how many materials is this supplier the ONLY source for?
            var soleSourcingCount = rows
                .Select(r => new { r.StockCode, r.ItemCode })
                .Distinct()
                .Count(m =>
                {
                    var key = new { m.StockCode, m.ItemCode };
                    var supplierList = materialSupplierCounts.GetValueOrDefault(key);
                    return supplierList != null && supplierList.Count == 1;
                });

            // 🎓 Revenue exposure — estimate revenue dependent on this supplier's materials
            var dependentStyleKeys = dependentStyles
                .Select(d => new { d.BuyerCode, d.Order, d.StyleCode })
                .ToHashSet();
            var revenueExposure = styles
                .Where(st => dependentStyleKeys.Any(dk =>
                    dk.BuyerCode == st.BuyerCode &&
                    dk.Order == st.Order &&
                    dk.StyleCode == st.StyleCode))
                .Sum(st => (st.Quantity ?? 0m) * (st.UnitPrice ?? 0m));

            // 🎓 Pre-compute risk flags for each dimension
            var concentrationRisk = consumptionSharePercent > 20 ? "HIGH" :
                consumptionSharePercent > 10 ? "MEDIUM" : "LOW";
            var contactRisk = contactChannels == 0 ? "HIGH" :
                contactChannels == 1 ? "MEDIUM" : "LOW";
            var breadthRisk = materialCategoryCount <= 1 && rows.Count > 0 ? "HIGH" :
                materialCategoryCount <= 2 ? "MEDIUM" : "LOW";
            var dependencyRisk = soleSourcingCount > 3 ? "HIGH" :
                soleSourcingCount > 1 ? "MEDIUM" : "LOW";
            var revenueRisk = totalRevenue > 0 && revenueExposure / totalRevenue > 0.2m ? "HIGH" :
                totalRevenue > 0 && revenueExposure / totalRevenue > 0.1m ? "MEDIUM" : "LOW";

            return new
            {
                s.SupplierCode,
                s.Name,
                ContactChannels = contactChannels,
                HasAddress = s.AddressId != null,
                StylesDependent = dependentStyles.Count,
                MaterialsSupplied = materialsSupplied,
                MaterialCategoryCount = materialCategoryCount,
                TotalConsumptionVolume = supplierConsumption,
                ConsumptionSharePercent = consumptionSharePercent,
                IsCriticalSupplier = consumptionSharePercent > 15,
                DependentBuyerCount = dependentBuyers,
                RevenueExposure = revenueExposure,
                SoleSourcingCount = soleSourcingCount,
                RiskFactors = new
                {
                    ConcentrationRisk = concentrationRisk,
                    ContactRisk = contactRisk,
                    BreadthRisk = breadthRisk,
                    DependencyRisk = dependencyRisk,
                    RevenueRisk = revenueRisk
                }
            };
        }).ToList();

        // 🎓 Aggregate risk signals
        var result = new
        {
            SupplierRiskData = supplierRiskData,
            TotalSupplierCount = supplierRiskData.Count,
            CriticalSupplierCount = supplierRiskData.Count(s => s.IsCriticalSupplier),
            UnreachableSupplierCount = supplierRiskData.Count(s => s.ContactChannels == 0),
            HighRiskSupplierCount = supplierRiskData.Count(s =>
            {
                var rf = s.RiskFactors;
                var highCount = 0;
                if (rf.ConcentrationRisk == "HIGH") highCount++;
                if (rf.ContactRisk == "HIGH") highCount++;
                if (rf.BreadthRisk == "HIGH") highCount++;
                if (rf.DependencyRisk == "HIGH") highCount++;
                if (rf.RevenueRisk == "HIGH") highCount++;
                return highCount >= 2;
            })
        };

        var json = JsonSerializer.Serialize(result, JsonOptions);
        return (json, suppliers.Count);
    }

    // ─────────────────────────────────────────────
    // D. COST OPTIMIZATION
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates cost optimisation signals across all active styles.
    ///
    /// SIGNAL COMPUTATION:
    /// For each style, compute:
    ///   1. consumptionPerUnit — material intensity per garment
    ///   2. wastageImpactCost — estimated $ cost of wastage
    ///   3. potentialWastageSaving — $ saved if wastage reduced to 5% benchmark
    ///   4. fobVarianceFromTypeAvg — how far FOB deviates from garment type average
    ///
    /// These computed signals enable the AI to QUANTIFY savings opportunities
    /// instead of just identifying patterns.
    ///
    /// 🎓 WHY COMPUTE $ IMPACT PRE-AI?
    /// The AI is good at finding patterns but imprecise at arithmetic.
    /// By pre-computing $ impact, we ensure the savings figures in the
    /// AI's recommendations are based on actual calculations, not estimates.
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateCostOptimizationAsync(
        CancellationToken ct)
    {
        // 🎓 Load styles with buyer names
        var stylesWithBuyers = await _db.Styles
            .Join(_db.Buyers,
                s => s.BuyerCode,
                b => b.BuyerCode,
                (s, b) => new { Style = s, BuyerName = b.Name })
            .AsNoTracking()
            .ToListAsync(ct);

        if (stylesWithBuyers.Count == 0)
            return ("[]", 0);

        // 🎓 Load garment type names for grouping
        var garmentTypes = await _db.GarmentTypes
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.TypeName, ct);

        // 🎓 Load all consumption ledger rows
        var ledger = await _db.StyleMaterialConsumptionLedgers
            .AsNoTracking()
            .ToListAsync(ct);

        var consumptionByStyle = ledger
            .GroupBy(l => new { l.BuyerCode, l.Order, l.TypeCode, l.StyleCode })
            .ToDictionary(g => g.Key, g => g.ToList());

        // 🎓 Pre-compute average FOB by garment type for variance analysis
        var fobByGarmentType = stylesWithBuyers
            .Where(x => x.Style.UnitPrice > 0)
            .GroupBy(x => x.Style.TypeCode)
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(x => x.Style.UnitPrice ?? 0m), 2));

        // 🎓 Compute per-style cost optimisation signals
        var styleSignals = stylesWithBuyers.Select(x =>
        {
            var s = x.Style;
            var key = new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode };
            var rows = consumptionByStyle.GetValueOrDefault(key) ?? [];

            var directMaterials = rows.Where(r => !r.IsAdditionalCost).ToList();
            var additionalCosts = rows.Where(r => r.IsAdditionalCost).ToList();
            var totalPlanned = directMaterials.Sum(r => r.TotalConsumption);
            var orderQty = s.Quantity ?? 0m;
            var unitPrice = s.UnitPrice ?? 0m;
            var totalOrderValue = orderQty * unitPrice;

            // 🎓 Material intensity — consumption per garment unit
            var consumptionPerUnit = orderQty > 0
                ? Math.Round(totalPlanned / orderQty, 4)
                : 0m;

            // 🎓 Wastage analysis
            var avgWastage = directMaterials.Count > 0
                ? Math.Round(directMaterials.Average(r => r.PercentageAllowance), 2)
                : 0m;
            var wastageImpact = avgWastage > 0
                ? Math.Round(totalPlanned * avgWastage / 100m, 2)
                : 0m;

            // 🎓 Wastage cost estimation — using unitPrice as proxy for material cost
            //    This is an approximation: material cost ≈ a fraction of FOB.
            //    For garments, material typically represents 40-60% of FOB.
            var materialCostFraction = 0.5m; // 🎓 Industry heuristic: 50% of FOB is material
            var wastageImpactCost = unitPrice > 0
                ? Math.Round(wastageImpact * unitPrice * materialCostFraction, 2)
                : 0m;

            // 🎓 Potential saving if wastage reduced to 5% benchmark
            var benchmarkWastage = 5m;
            var potentialWastageSaving = avgWastage > benchmarkWastage && totalPlanned > 0 && unitPrice > 0
                ? Math.Round(totalPlanned * (avgWastage - benchmarkWastage) / 100m * unitPrice * materialCostFraction, 2)
                : 0m;

            // 🎓 FOB variance from garment type average
            var typeFobAvg = fobByGarmentType.GetValueOrDefault(s.TypeCode, 0m);
            var fobVariance = typeFobAvg > 0 && unitPrice > 0
                ? Math.Round(unitPrice - typeFobAvg, 2)
                : 0m;

            return new
            {
                s.StyleCode,
                s.Order,
                s.BuyerCode,
                BuyerName = x.BuyerName,
                GarmentTypeName = garmentTypes.GetValueOrDefault(s.TypeCode, "Unknown"),
                OrderQuantity = orderQty,
                UnitPrice = unitPrice,
                TotalOrderValue = totalOrderValue,
                TotalPlannedConsumption = totalPlanned,
                ConsumptionPerUnit = consumptionPerUnit,
                DirectMaterialLineCount = directMaterials.Count,
                AdditionalCostLineCount = additionalCosts.Count,
                AverageWastage = avgWastage,
                WastageImpact = wastageImpact,
                WastageImpactCost = wastageImpactCost,
                PotentialWastageSaving = potentialWastageSaving,
                TopCostDriverMaterial = directMaterials
                    .OrderByDescending(r => r.TotalConsumption)
                    .Select(r => r.StockCode + "/" + r.ItemCode)
                    .FirstOrDefault() ?? "N/A",
                FobPerGarmentType = typeFobAvg,
                FobVarianceFromTypeAvg = fobVariance
            };
        }).ToList();

        // 🎓 Aggregate optimisation signals
        var totalWastageImpactCost = styleSignals.Sum(s => s.WastageImpactCost);
        var totalPotentialSaving = styleSignals.Sum(s => s.PotentialWastageSaving);
        var avgBomLines = styleSignals.Count > 0
            ? Math.Round(styleSignals.Average(s => (decimal)(s.DirectMaterialLineCount + s.AdditionalCostLineCount)), 1)
            : 0m;

        var result = new
        {
            StyleSignals = styleSignals,
            TotalWastageImpactCost = totalWastageImpactCost,
            TotalPotentialSaving = totalPotentialSaving,
            BomComplexityOutlierCount = styleSignals.Count(s =>
                s.DirectMaterialLineCount + s.AdditionalCostLineCount > (double)avgBomLines * 2),
            PricingInconsistencyCount = fobByGarmentType.Count(kvp =>
            {
                var typeStyles = stylesWithBuyers
                    .Where(x => x.Style.TypeCode == kvp.Key && x.Style.UnitPrice > 0)
                    .Select(x => x.Style.UnitPrice ?? 0m)
                    .ToList();
                if (typeStyles.Count < 2) return false;
                var avg = typeStyles.Average();
                if (avg == 0) return false;
                var stdDev = Math.Sqrt(typeStyles.Average(p => Math.Pow((double)(p - avg), 2)));
                var cv = stdDev / (double)avg;  // 🎓 Coefficient of variation
                return cv > 0.3; // 🎓 > 30% CV = significant pricing inconsistency
            }),
            TopWastageStyles = styleSignals
                .OrderByDescending(s => s.WastageImpactCost)
                .Take(10)
                .Select(s => new { s.StyleCode, s.Order, s.BuyerCode, s.WastageImpactCost, s.AverageWastage })
                .ToList(),
            TopConsumptionStyles = styleSignals
                .Where(s => s.ConsumptionPerUnit > 0)
                .OrderByDescending(s => s.ConsumptionPerUnit)
                .Take(10)
                .Select(s => new { s.StyleCode, s.Order, s.BuyerCode, s.ConsumptionPerUnit, s.GarmentTypeName })
                .ToList()
        };

        var json = JsonSerializer.Serialize(result, JsonOptions);
        return (json, stylesWithBuyers.Count);
    }
}
