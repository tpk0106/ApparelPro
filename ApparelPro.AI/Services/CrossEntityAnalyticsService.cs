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
// 🎓 CROSS-ENTITY ANALYTICS SERVICE — IMPLEMENTATION
// Phase 2, Feature 1: Aggregates data across multiple entities from EF Core,
// serializes it to JSON, and sends it to the AI with category-specific prompts.
//
// 🎓 ARCHITECTURE DECISIONS:
//
// 1. REGISTERED AS TRANSIENT (not singleton)
//    Unlike AnomalyDetectionService (singleton + IServiceScopeFactory for background
//    scanning), this service is called per-request from the controller. Transient
//    registration lets it directly inject the scoped ApparelProDbContext — simpler
//    and more aligned with how StyleDetailsService, BuyerService etc. work.
//
// 2. COMPOSES IAiService (doesn't extend it)
//    This service owns the data aggregation logic and delegates AI completion to
//    IAiService.CompleteAsync(). This keeps IAiService clean (SRP) and lets us
//    swap AI providers without touching aggregation code.
//
// 3. AGGREGATION QUERIES USE AsNoTracking()
//    All queries are read-only aggregations — no entity tracking needed.
//    AsNoTracking() reduces memory pressure, especially important when loading
//    50+ styles' worth of consumption data.
//
// 4. DATA SHAPE MATCHES PROMPT TEMPLATES
//    Each aggregation method returns an anonymous object whose properties match
//    EXACTLY what the corresponding prompt template in AnalyticsPromptTemplates.cs
//    tells the AI to expect. If you change a template's expected fields, update
//    the matching aggregation method here.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Aggregates cross-entity data and sends it to the AI for strategic analysis.
/// Registered as Transient — injects scoped DbContext directly.
/// </summary>
public sealed class CrossEntityAnalyticsService : ICrossEntityAnalyticsService
{
    private readonly ApparelProDbContext _db;
    private readonly IAiService _aiService;
    private readonly AiSettings _settings;
    private readonly ILogger<CrossEntityAnalyticsService> _logger;

    // 🎓 JSON serialization options — camelCase for consistency with frontend
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CrossEntityAnalyticsService(
        ApparelProDbContext db,
        IAiService aiService,
        IOptions<AiSettings> settings,
        ILogger<CrossEntityAnalyticsService> logger)
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
    public async Task<(AiCompletionResponse Response, int EntityCount)> GetAnalyticsAsync(
        string category,
        string? userQuery = null,
        CancellationToken cancellationToken = default)
    {
        // 🎓 STEP 1: Validate category — GetAnalyticsTemplate will throw
        //            ArgumentException for unsupported categories.
        var normalizedCategory = category.Trim().ToUpperInvariant();
        var template = AnalyticsPromptTemplates.GetAnalyticsTemplate(normalizedCategory);

        // 🎓 STEP 2: Aggregate data for this category from EF Core
        var (aggregatedJson, entityCount) = await AggregateDataAsync(
            normalizedCategory, cancellationToken);

        if (entityCount == 0)
        {
            _logger.LogWarning(
                "No data found for analytics category {Category}. " +
                "Returning empty analysis.", normalizedCategory);

            return (new AiCompletionResponse
            {
                Content = "No data available for this analytics category. " +
                          "Please ensure there are active records in the system.",
                Provider = "None",
                Model = "None",
                TotalTokens = 0,
                InputTokens = 0,
                OutputTokens = 0
            }, 0);
        }

        // 🎓 STEP 3: Build the user message from the prompt template
        //            {0} = aggregated JSON data, {1} = optional user query
        var userQuerySection = string.IsNullOrWhiteSpace(userQuery)
            ? string.Empty
            : $"Additionally, focus on this question: {userQuery}";

        var userMessage = string.Format(template, aggregatedJson, userQuerySection);

        // 🎓 STEP 4: Build the AI completion request
        //            Uses the analytics-specific system prompt (strategic analyst persona)
        //            and AnalyticsMaxTokens (3500) — analytics responses need more room
        //            than single-entity analysis (2500) because they cover MORE data.
        var request = new AiCompletionRequest
        {
            SystemPrompt = AnalyticsPromptTemplates.AnalyticsSystem,
            UserMessage = userMessage,
            MaxTokens = _settings.AnalyticsMaxTokens,
            Temperature = 0.25  // Low temperature — we want factual, consistent analysis
        };

        _logger.LogInformation(
            "Running cross-entity analytics for category {Category} " +
            "across {EntityCount} entities via {Provider}",
            normalizedCategory, entityCount, _settings.ActiveProvider);

        // 🎓 STEP 5: Call IAiService.CompleteAsync() — delegates to the active provider
        var response = await _aiService.CompleteAsync(request, cancellationToken);

        _logger.LogInformation(
            "Analytics complete for {Category}. " +
            "Input: {InputTokens}, Output: {OutputTokens}, Total: {TotalTokens}",
            normalizedCategory, response.InputTokens,
            response.OutputTokens, response.TotalTokens);

        return (response, entityCount);
    }

    // ═══════════════════════════════════════════════════════════
    // DATA AGGREGATION — one method per category
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 🎓 Dispatcher: routes to the correct aggregation method based on category.
    /// Returns (serialized JSON string, entity count).
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateDataAsync(
        string category,
        CancellationToken ct)
    {
        return category switch
        {
            "MATERIAL_CONSUMPTION" => await AggregateMaterialConsumptionAsync(ct),
            "SUPPLIER_PERFORMANCE" => await AggregateSupplierPerformanceAsync(ct),
            "BUYER_PORTFOLIO"      => await AggregateBuyerPortfolioAsync(ct),
            "ORDER_PIPELINE"       => await AggregateOrderPipelineAsync(ct),
            "COST_ANALYSIS"        => await AggregateCostAnalysisAsync(ct),
            _ => throw new ArgumentException($"Unsupported analytics category: '{category}'.")
        };
    }

    // ─────────────────────────────────────────────
    // A. MATERIAL CONSUMPTION
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates material consumption data across all active styles.
    ///
    /// QUERY STRATEGY:
    /// 1. Load all styles (with buyer name and garment type) — the "parent" records
    /// 2. Load all consumption ledger rows — the "child" records
    /// 3. Group consumption by style key and compute per-style metrics:
    ///    - Total BOM lines, direct material lines, additional cost lines
    ///    - Total planned consumption, average/max wastage
    ///    - Supplier coverage (% of materials with a supplier assigned)
    ///    - Top material by consumption volume
    ///
    /// 🎓 WHY NOT A SINGLE JOIN QUERY?
    /// EF Core struggles with complex GroupBy + aggregation on joined tables
    /// (often falls back to client-side evaluation). Two separate queries +
    /// in-memory grouping is more reliable and still fast for typical datasets
    /// (hundreds of styles, not millions).
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateMaterialConsumptionAsync(
        CancellationToken ct)
    {
        // 🎓 Load all styles with their buyer names
        var styles = await _db.Styles
            .Join(_db.Buyers,
                s => s.BuyerCode,
                b => b.BuyerCode,
                (s, b) => new
                {
                    s.BuyerCode,
                    s.Order,
                    s.TypeCode,
                    s.StyleCode,
                    BuyerName = b.Name,
                    s.Quantity,
                    s.UnitPrice
                })
            .AsNoTracking()
            .ToListAsync(ct);

        if (styles.Count == 0)
            return ("[]", 0);

        // 🎓 Load all consumption ledger rows
        var ledger = await _db.StyleMaterialConsumptionLedgers
            .AsNoTracking()
            .ToListAsync(ct);

        // 🎓 Group consumption by style key
        var consumptionByStyle = ledger
            .GroupBy(l => new { l.BuyerCode, l.Order, l.TypeCode, l.StyleCode })
            .ToDictionary(
                g => g.Key,
                g => g.ToList());

        // 🎓 Build the aggregated data — one entry per style
        var aggregated = styles.Select(s =>
        {
            var key = new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode };
            var rows = consumptionByStyle.GetValueOrDefault(key) ?? [];

            var directMaterials = rows.Where(r => !r.IsAdditionalCost).ToList();
            var additionalCosts = rows.Where(r => r.IsAdditionalCost).ToList();

            return new
            {
                s.StyleCode,
                s.Order,
                BuyerCode = s.BuyerCode,
                s.BuyerName,
                OrderQuantity = s.Quantity ?? 0m,
                UnitPrice = s.UnitPrice ?? 0m,
                TotalBomLines = rows.Count,
                DirectMaterialLines = directMaterials.Count,
                AdditionalCostLines = additionalCosts.Count,
                TotalPlannedConsumption = directMaterials.Sum(r => r.TotalConsumption),
                AverageWastagePercent = directMaterials.Count > 0
                    ? Math.Round(directMaterials.Average(r => r.PercentageAllowance), 2)
                    : 0m,
                MaxWastagePercent = directMaterials.Count > 0
                    ? directMaterials.Max(r => r.PercentageAllowance)
                    : 0m,
                SupplierCoverage = directMaterials.Count > 0
                    ? Math.Round(
                        (decimal)directMaterials.Count(r =>
                            !string.IsNullOrWhiteSpace(r.SupplierCode) &&
                            r.SupplierCode != "0")
                        / directMaterials.Count * 100m, 1)
                    : 0m,
                TopMaterialByConsumption = directMaterials
                    .OrderByDescending(r => r.TotalConsumption)
                    .Select(r => r.StockCode + "/" + r.ItemCode)
                    .FirstOrDefault() ?? "N/A",
                EstimatedMaterialCost = directMaterials.Sum(r => r.TotalConsumption)
            };
        }).ToList();

        var json = JsonSerializer.Serialize(aggregated, JsonOptions);
        return (json, aggregated.Count);
    }

    // ─────────────────────────────────────────────
    // B. SUPPLIER PERFORMANCE
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates supplier distribution and coverage across all styles/materials.
    ///
    /// Includes:
    ///   - Per-supplier: styles supplied, material categories, total consumption volume
    ///   - Contact completeness (phone, mobile, fax channels)
    ///   - Unassigned materials (consumption rows with no supplier)
    ///   - Supplier concentration (top 3 suppliers' share of total consumption)
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateSupplierPerformanceAsync(
        CancellationToken ct)
    {
        // 🎓 Load all suppliers
        var suppliers = await _db.Suppliers
            .AsNoTracking()
            .ToListAsync(ct);

        if (suppliers.Count == 0)
            return ("[]", 0);

        // 🎓 Load all direct material consumption rows (not additional costs)
        var ledger = await _db.StyleMaterialConsumptionLedgers
            .Where(l => !l.IsAdditionalCost)
            .AsNoTracking()
            .ToListAsync(ct);

        var totalConsumption = ledger.Sum(l => l.TotalConsumption);

        // 🎓 Group consumption by supplier code
        var consumptionBySupplier = ledger
            .Where(l => !string.IsNullOrWhiteSpace(l.SupplierCode) && l.SupplierCode != "0")
            .GroupBy(l => l.SupplierCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 🎓 Unassigned materials — consumption rows with no supplier
        var unassignedMaterials = ledger
            .Where(l => string.IsNullOrWhiteSpace(l.SupplierCode) || l.SupplierCode == "0")
            .Select(l => new
            {
                l.StockCode,
                l.ItemCode,
                l.StyleCode,
                l.Order
            })
            .ToList();

        // 🎓 Build per-supplier aggregation
        var supplierData = suppliers.Select(s =>
        {
            var supplierCodeStr = s.SupplierCode.ToString();
            var rows = consumptionBySupplier.GetValueOrDefault(supplierCodeStr) ?? [];

            // 🎓 Count contact channels — each non-empty contact field is one channel
            var contactChannels = 0;
            if (!string.IsNullOrWhiteSpace(s.TelephoneNos)) contactChannels++;
            if (!string.IsNullOrWhiteSpace(s.MobileNos)) contactChannels++;
            if (!string.IsNullOrWhiteSpace(s.Fax)) contactChannels++;

            return new
            {
                s.SupplierCode,
                s.Name,
                ContactChannels = contactChannels,
                HasAddress = s.AddressId != null,
                StylesSupplied = rows
                    .Select(r => new { r.BuyerCode, r.Order, r.TypeCode, r.StyleCode })
                    .Distinct().Count(),
                MaterialsSupplied = rows
                    .Select(r => r.StockCode + "/" + r.ItemCode)
                    .Distinct().ToList(),
                MaterialCategoryCount = rows
                    .Select(r => r.StockCode)
                    .Distinct().Count(),
                TotalConsumptionVolume = rows.Sum(r => r.TotalConsumption)
            };
        }).ToList();

        // 🎓 Supplier concentration — top 3 suppliers' share
        var top3Consumption = supplierData
            .OrderByDescending(s => s.TotalConsumptionVolume)
            .Take(3)
            .Sum(s => s.TotalConsumptionVolume);

        var supplierConcentration = totalConsumption > 0
            ? Math.Round(top3Consumption / totalConsumption * 100m, 1)
            : 0m;

        var result = new
        {
            Suppliers = supplierData,
            UnassignedMaterials = unassignedMaterials,
            SupplierConcentration = supplierConcentration
        };

        var json = JsonSerializer.Serialize(result, JsonOptions);
        return (json, suppliers.Count);
    }

    // ─────────────────────────────────────────────
    // C. BUYER PORTFOLIO
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates buyer portfolio data — order volumes, revenue distribution,
    /// pricing patterns, and customer concentration.
    ///
    /// Joins Buyers → PurchaseOrders → Styles to compute:
    ///   - Total orders, total styles, total quantities per buyer
    ///   - Total estimated revenue (quantity × unitPrice)
    ///   - Average FOB price across styles
    ///   - Contact completeness and delivery address presence
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateBuyerPortfolioAsync(
        CancellationToken ct)
    {
        var buyers = await _db.Buyers
            .AsNoTracking()
            .ToListAsync(ct);

        if (buyers.Count == 0)
            return ("[]", 0);

        var orders = await _db.PurchaseOrders
            .AsNoTracking()
            .ToListAsync(ct);

        var styles = await _db.Styles
            .AsNoTracking()
            .ToListAsync(ct);

        var aggregated = buyers.Select(b =>
        {
            var buyerOrders = orders.Where(o => o.BuyerCode == b.BuyerCode).ToList();
            var buyerStyles = styles.Where(s => s.BuyerCode == b.BuyerCode).ToList();

            var totalOrderQuantity = buyerStyles.Sum(s => s.Quantity ?? 0m);
            var totalEstimatedRevenue = buyerStyles.Sum(s =>
                (s.Quantity ?? 0m) * (s.UnitPrice ?? 0m));
            var averageFobPrice = buyerStyles.Count > 0 && buyerStyles.Any(s => s.UnitPrice > 0)
                ? Math.Round(
                    buyerStyles.Where(s => s.UnitPrice > 0)
                        .Average(s => s.UnitPrice ?? 0m), 2)
                : 0m;

            // 🎓 Contact completeness — count non-empty contact fields
            var contactCompleteness = 0;
            if (!string.IsNullOrWhiteSpace(b.TelephoneNos)) contactCompleteness++;
            if (!string.IsNullOrWhiteSpace(b.MobileNos)) contactCompleteness++;
            if (!string.IsNullOrWhiteSpace(b.Fax)) contactCompleteness++;

            return new
            {
                b.BuyerCode,
                BuyerName = b.Name,
                Status = b.Status,
                TotalOrders = buyerOrders.Count,
                TotalStyles = buyerStyles.Count,
                TotalOrderQuantity = totalOrderQuantity,
                TotalEstimatedRevenue = totalEstimatedRevenue,
                AverageFobPrice = averageFobPrice,
                ContactCompleteness = contactCompleteness,
                HasDeliveryAddress = b.Addresses.Any()
            };
        }).ToList();

        var json = JsonSerializer.Serialize(aggregated, JsonOptions);
        return (json, aggregated.Count);
    }

    // ─────────────────────────────────────────────
    // D. ORDER PIPELINE
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates purchase order pipeline data across the business.
    ///
    /// For each PO, computes:
    ///   - Order value (totalQuantity × basisValue)
    ///   - Days since order date (age indicator)
    ///   - Whether description exists and whether quantity was overridden
    ///   - Garment type name (joined from GarmentTypes table)
    ///   - Buyer name (joined from Buyers table)
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateOrderPipelineAsync(
        CancellationToken ct)
    {
        var orders = await _db.PurchaseOrders
            .AsNoTracking()
            .ToListAsync(ct);

        if (orders.Count == 0)
            return ("[]", 0);

        var buyers = await _db.Buyers
            .AsNoTracking()
            .ToDictionaryAsync(b => b.BuyerCode, b => b.Name, ct);

        var garmentTypes = await _db.GarmentTypes
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.TypeName, ct);

        var today = DateTime.UtcNow;

        var aggregated = orders.Select(o =>
        {
            var orderValue = o.TotalQuantity * o.BasisValue;
            var daysSinceOrder = (today - o.OrderDate).Days;

            return new
            {
                o.BuyerCode,
                BuyerName = buyers.GetValueOrDefault(o.BuyerCode, "Unknown"),
                o.Order,
                GarmentTypeName = garmentTypes.GetValueOrDefault(o.GarmentType, "Unknown"),
                o.TotalQuantity,
                o.CurrencyCode,
                o.BasisCode,
                o.BasisValue,
                OrderValue = orderValue,
                o.Season,
                OrderDate = o.OrderDate.ToString("yyyy-MM-dd"),
                DaysSinceOrder = daysSinceOrder,
                HasDescription = !string.IsNullOrWhiteSpace(o.Description),
                QuantityOverridden = o.QuantityOverriddenBy != null
            };
        }).ToList();

        var json = JsonSerializer.Serialize(aggregated, JsonOptions);
        return (json, aggregated.Count);
    }

    // ─────────────────────────────────────────────
    // E. COST ANALYSIS
    // ─────────────────────────────────────────────

    /// <summary>
    /// 🎓 Aggregates cost structures across all active styles.
    ///
    /// Combines style economics (quantity × unitPrice) with consumption data
    /// to build a cost picture per style:
    ///   - Total order value, total planned consumption
    ///   - Direct material vs additional cost line counts
    ///   - Average wastage % and estimated wastage impact
    ///   - Top cost driver material (highest consumption)
    ///
    /// 🎓 WHY WASTAGE IMPACT MATTERS:
    /// wastageImpact = totalPlannedConsumption × (averageWastage / 100)
    /// This estimates how much extra material is consumed JUST due to wastage
    /// allowances. If the business-wide wastage impact is high, reducing
    /// wastage on the worst offenders is a direct cost saving.
    /// </summary>
    private async Task<(string Json, int EntityCount)> AggregateCostAnalysisAsync(
        CancellationToken ct)
    {
        // 🎓 Load styles with buyer + garment type joins
        var styles = await _db.Styles
            .Join(_db.Buyers,
                s => s.BuyerCode,
                b => b.BuyerCode,
                (s, b) => new { Style = s, BuyerName = b.Name })
            .AsNoTracking()
            .ToListAsync(ct);

        if (styles.Count == 0)
            return ("[]", 0);

        var garmentTypes = await _db.GarmentTypes
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.TypeName, ct);

        var ledger = await _db.StyleMaterialConsumptionLedgers
            .AsNoTracking()
            .ToListAsync(ct);

        var consumptionByStyle = ledger
            .GroupBy(l => new { l.BuyerCode, l.Order, l.TypeCode, l.StyleCode })
            .ToDictionary(g => g.Key, g => g.ToList());

        var aggregated = styles.Select(x =>
        {
            var s = x.Style;
            var key = new { s.BuyerCode, s.Order, s.TypeCode, s.StyleCode };
            var rows = consumptionByStyle.GetValueOrDefault(key) ?? [];

            var directMaterials = rows.Where(r => !r.IsAdditionalCost).ToList();
            var additionalCosts = rows.Where(r => r.IsAdditionalCost).ToList();
            var totalPlanned = directMaterials.Sum(r => r.TotalConsumption);
            var avgWastage = directMaterials.Count > 0
                ? Math.Round(directMaterials.Average(r => r.PercentageAllowance), 2)
                : 0m;

            return new
            {
                s.StyleCode,
                s.Order,
                s.BuyerCode,
                BuyerName = x.BuyerName,
                GarmentTypeName = garmentTypes.GetValueOrDefault(s.TypeCode, "Unknown"),
                OrderQuantity = s.Quantity ?? 0m,
                UnitPrice = s.UnitPrice ?? 0m,
                TotalOrderValue = (s.Quantity ?? 0m) * (s.UnitPrice ?? 0m),
                TotalPlannedConsumption = totalPlanned,
                DirectMaterialLineCount = directMaterials.Count,
                AdditionalCostLineCount = additionalCosts.Count,
                AverageWastage = avgWastage,
                // 🎓 Wastage impact = how much extra consumption is attributable to wastage
                WastageImpact = avgWastage > 0
                    ? Math.Round(totalPlanned * avgWastage / 100m, 2)
                    : 0m,
                TopCostDriverMaterial = directMaterials
                    .OrderByDescending(r => r.TotalConsumption)
                    .Select(r => r.StockCode + "/" + r.ItemCode)
                    .FirstOrDefault() ?? "N/A"
            };
        }).ToList();

        var json = JsonSerializer.Serialize(aggregated, JsonOptions);
        return (json, aggregated.Count);
    }
}
