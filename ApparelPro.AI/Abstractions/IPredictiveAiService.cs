using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 PREDICTIVE AI SERVICE — INTERFACE
// Phase 5: Predictive & Prescriptive AI
//
// 🎓 WHY A SEPARATE INTERFACE (not adding to ICrossEntityAnalyticsService)?
// Single Responsibility Principle (SRP):
//   - ICrossEntityAnalyticsService handles BACKWARD-LOOKING analysis
//     ("What happened? What are the patterns?")
//   - IPredictiveAiService handles FORWARD-LOOKING predictions
//     ("What will happen? What should we do about it?")
//
// The predictive service has a fundamentally different job:
//   1. Query EF Core for HISTORICAL data (trends over time, not snapshots)
//   2. Compute derived signals (growth rates, risk scores, shortage indicators)
//   3. Build prediction-specific prompts that ask the AI to FORECAST, not analyse
//   4. Return structured predictions with confidence levels and time horizons
//
// 🎓 FOUR PREDICTION CATEGORIES:
//
//   DEMAND_FORECASTING — Predicts future order quantities by analysing
//     historical buyer ordering patterns, seasonal trends, and style demand.
//     Uses: PurchaseOrders, Styles, Buyers (order dates, quantities, seasons)
//
//   MATERIAL_SHORTAGE — Identifies materials at risk of shortage by comparing
//     planned consumption across upcoming orders against supplier coverage
//     and historical supply patterns.
//     Uses: StyleMaterialConsumptionLedgers, Suppliers, Styles, PurchaseOrders
//
//   SUPPLIER_RISK — Scores each supplier on risk factors: concentration of
//     supply, contact completeness, material category breadth, number of
//     dependent styles, and geographic diversification.
//     Uses: Suppliers, StyleMaterialConsumptionLedgers, Styles
//
//   COST_OPTIMIZATION — Recommends specific cost reduction actions by
//     analysing wastage patterns, BOM complexity, pricing inconsistencies,
//     and material substitution opportunities.
//     Uses: Styles, StyleMaterialConsumptionLedgers, Buyers, PurchaseOrders
//
// 🎓 DEPENDENCY INVERSION PRINCIPLE (DIP):
// The controller depends on THIS interface, never the concrete implementation.
// If we later swap the prediction engine (e.g. add ML models alongside AI),
// only the implementation changes — controller and tests remain untouched.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Service for Phase 5 Predictive &amp; Prescriptive AI.
/// Aggregates historical data, computes risk/trend signals, and sends
/// them to the AI for forward-looking predictions and recommendations.
/// </summary>
public interface IPredictiveAiService
{
    /// <summary>
    /// 🎓 Run a predictive analysis for the specified category.
    ///
    /// The flow:
    ///   1. Validate the prediction category string
    ///   2. Query EF Core to aggregate HISTORICAL data for that category
    ///   3. Compute derived signals (growth rates, risk scores, shortage indicators)
    ///   4. Serialize data + signals to JSON
    ///   5. Build the prompt from PredictivePromptTemplates
    ///   6. Call IAiService.CompleteAsync() with the predictive system prompt
    ///   7. Return the AI response + metadata (entity count, category)
    ///
    /// Categories: DEMAND_FORECASTING, MATERIAL_SHORTAGE,
    ///             SUPPLIER_RISK, COST_OPTIMIZATION
    /// </summary>
    /// <param name="category">The prediction category to run (case-insensitive).</param>
    /// <param name="userQuery">Optional user question to focus the prediction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple of (AiCompletionResponse, entityCount) where entityCount is
    /// the number of entities included in the aggregated dataset.
    /// </returns>
    Task<(AiCompletionResponse Response, int EntityCount)> GetPredictionAsync(
        string category,
        string? userQuery = null,
        CancellationToken cancellationToken = default);
}
