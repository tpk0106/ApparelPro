namespace ApparelPro.AI.Models;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 CROSS-ENTITY ANALYTICS — API REQUEST MODEL
// Phase 2, Feature 1: The request body for POST /api/ai/analytics.
//
// 🎓 WHY A DIFFERENT MODEL FROM AiSummariseAPIModel?
// Summarise/Analyse operates on a SINGLE entity identified by EntityType + EntityKey
// (e.g. "Style", "1/ORD001/2/ST001"). Cross-Entity Analytics operates on an entire
// CATEGORY of data across ALL entities — there's no single entity key. Instead, the
// caller picks a category (like "MATERIAL_CONSUMPTION") and the service aggregates
// data from the database itself.
//
// The two request shapes are fundamentally different:
//   - Single entity:  EntityType + EntityKey → "find this one record"
//   - Cross-entity:   Category → "aggregate ALL records in this category"
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// API-level request model for the cross-entity analytics endpoint.
/// The caller specifies which analytics category to run — the service
/// handles all data aggregation internally.
/// </summary>
public sealed class CrossEntityAnalyticsAPIModel
{
    /// <summary>
    /// 🎓 The analytics category to run.
    /// Each category maps to a different data aggregation query AND a specialised
    /// prompt template in AnalyticsPromptTemplates.cs.
    ///
    /// Supported values (case-insensitive):
    ///   MATERIAL_CONSUMPTION — consumption patterns, wastage, cost drivers across styles
    ///   SUPPLIER_PERFORMANCE — supplier coverage, order distribution, contact completeness
    ///   BUYER_PORTFOLIO      — buyer order volumes, style counts, revenue concentration
    ///   ORDER_PIPELINE       — purchase order status, value distribution, timeline analysis
    ///   COST_ANALYSIS        — cost structures, pricing patterns, margin indicators
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// 🎓 Optional user question to focus the analysis.
    /// If provided, the AI will address this specific question IN ADDITION to the
    /// standard analysis dimensions for the category.
    ///
    /// Examples:
    ///   "Which suppliers are single points of failure?"
    ///   "Show me styles with wastage above 8%"
    ///   "Which buyers drive 80% of revenue?"
    /// </summary>
    public string? UserQuery { get; init; }
}
