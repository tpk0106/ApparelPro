namespace ApparelPro.AI.Models;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 PREDICTIVE AI — API REQUEST & RESPONSE MODELS
// Phase 5: Predictive & Prescriptive AI
//
// 🎓 WHY SEPARATE MODELS FROM CrossEntityAnalyticsAPIModel?
// Cross-Entity Analytics and Predictive AI have different request/response shapes:
//
//   Analytics: { category, userQuery } → backward-looking analysis
//   Predictive: { category, userQuery, timeHorizonMonths } → forward-looking forecast
//
// The response is also different:
//   Analytics response → "Analysis" (what happened, patterns found)
//   Predictive response → "Prediction" (what will happen, recommended actions)
//
// 🎓 SHARED PATTERN:
// Both use the same token/cost tracking fields (Provider, Model, InputTokens,
// OutputTokens, TotalTokens, EstimatedCost, GeneratedAt) because both call
// IAiService.CompleteAsync() under the hood. The cost visibility is equally
// important for predictions since they aggregate large historical datasets.
// ─────────────────────────────────────────────────────────────────────────────

// ═══════════════════════════════════════════════════════════════════
// REQUEST MODEL
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// 🎓 API-level request model for the predictive AI endpoint.
/// The caller specifies which prediction category to run — the service
/// handles all data aggregation, signal computation, and AI orchestration.
/// </summary>
public sealed class PredictiveAiAPIModel
{
    /// <summary>
    /// 🎓 The prediction category to run.
    /// Each category maps to a different data aggregation query, signal
    /// computation pipeline, AND a specialised prompt template in
    /// PredictivePromptTemplates.cs.
    ///
    /// Supported values (case-insensitive):
    ///   DEMAND_FORECASTING  — future order quantity predictions from historical patterns
    ///   MATERIAL_SHORTAGE   — materials at risk of shortage based on consumption vs supply
    ///   SUPPLIER_RISK       — risk scores for each supplier based on multiple factors
    ///   COST_OPTIMIZATION   — specific cost reduction recommendations from BOM analysis
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// 🎓 Optional user question to focus the prediction.
    /// If provided, the AI will address this specific question IN ADDITION to the
    /// standard prediction dimensions for the category.
    ///
    /// Examples:
    ///   "What will buyer ALPHA's order volume look like next quarter?"
    ///   "Which materials are most likely to run short in the next 60 days?"
    ///   "Which suppliers should we develop backup sources for?"
    ///   "Where can we save the most on material costs?"
    /// </summary>
    public string? UserQuery { get; init; }

    /// <summary>
    /// 🎓 Optional time horizon for the prediction (in months).
    /// Defaults to 3 months if not specified.
    /// Used by DEMAND_FORECASTING and MATERIAL_SHORTAGE to scope the
    /// forecast window. SUPPLIER_RISK and COST_OPTIMIZATION use this
    /// to prioritise recommendations by urgency.
    ///
    /// Valid range: 1-12 months.
    /// </summary>
    public int? TimeHorizonMonths { get; init; }
}

// ═══════════════════════════════════════════════════════════════════
// RESPONSE MODEL
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// 🎓 API-level response model for the predictive AI endpoint.
/// Contains the AI-generated prediction plus token usage for cost tracking.
///
/// The Prediction field contains Markdown-formatted text with a consistent
/// structure across all categories:
///   1. PREDICTION SUMMARY — headline forecast with confidence level
///   2. DATA SIGNALS — computed signals the prediction is based on
///   3. DETAILED FORECAST — category-specific predictions with numbers
///   4. RISK FACTORS — what could change the prediction
///   5. RECOMMENDED ACTIONS — prescriptive steps ranked by impact
/// </summary>
public sealed class PredictiveAiAPIModel_Response
{
    /// <summary>
    /// 🎓 The AI-generated prediction content — structured with Prediction Summary,
    /// Data Signals, Detailed Forecast, Risk Factors, and Recommended Actions.
    /// This is Markdown-formatted text ready for rendering in the frontend.
    /// </summary>
    public required string Prediction { get; set; }

    /// <summary>
    /// The prediction category that was run (echoed back from the request).
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// 🎓 How many entities were included in the aggregated data sent to the AI.
    /// Context: "Prediction based on 47 styles' historical data" vs "3 styles".
    /// A low count might indicate insufficient history for reliable predictions.
    /// </summary>
    public int EntityCount { get; set; }

    /// <summary>
    /// 🎓 The time horizon (in months) that was used for this prediction.
    /// Echoed back so the frontend can display "3-month forecast" etc.
    /// </summary>
    public int TimeHorizonMonths { get; set; }

    /// <summary>
    /// Which AI provider generated this prediction (e.g. "Anthropic", "OpenAI").
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// The model identifier used (e.g. "claude-sonnet-4-20250514").
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Input tokens consumed — predictions may use even more tokens than analytics
    /// because historical trend data is larger than point-in-time snapshots.
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Output tokens generated — prediction responses are typically 2500-4000 tokens
    /// due to the additional forecast detail and action recommendations.
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Total tokens consumed (input + output).
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Estimated cost in USD for this AI call.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// When this prediction was generated (UTC).
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CATEGORY INFO MODEL (for the GET /categories endpoint)
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// 🎓 Describes an available prediction category for the frontend.
/// Used by the GET /api/ai/predictions/categories endpoint to populate
/// a dropdown/selector in the Predictions Dashboard.
/// </summary>
public sealed class PredictionCategoryInfo
{
    /// <summary>
    /// The category key to pass in the prediction request (e.g. "DEMAND_FORECASTING").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Human-readable display name (e.g. "Demand Forecasting").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Short description of what this category predicts.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// 🎓 Icon hint for the frontend — a Material UI icon name suggestion.
    /// The frontend can use this to render a category-specific icon.
    /// </summary>
    public required string IconHint { get; set; }
}
