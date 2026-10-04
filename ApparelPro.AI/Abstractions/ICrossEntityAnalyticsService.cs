using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 CROSS-ENTITY ANALYTICS SERVICE — INTERFACE
// Phase 2, Feature 1: Abstraction for the analytics service.
//
// 🎓 WHY A SEPARATE INTERFACE (not adding to IAiService)?
// Single Responsibility Principle (SRP):
//   - IAiService handles AI completion routing (summarise, analyse, chat)
//   - ICrossEntityAnalyticsService handles DATA AGGREGATION + AI orchestration
//
// The analytics service has a fundamentally different job:
//   1. Query EF Core to aggregate data across multiple entities
//   2. Serialize the aggregated data to JSON
//   3. Select the right prompt template for the category
//   4. Call IAiService.CompleteAsync() with the assembled prompt
//
// Steps 1-3 don't exist in IAiService. Cramming them in would violate SRP
// and make the interface unwieldy. Instead, this service COMPOSES IAiService
// (calls it internally) while owning the aggregation logic.
//
// 🎓 DEPENDENCY INVERSION PRINCIPLE (DIP):
// The controller depends on THIS interface, never the concrete implementation.
// If we later swap the aggregation source (e.g. from EF Core to a data warehouse),
// only the implementation changes — the controller and tests remain untouched.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Service for Phase 2 cross-entity analytics.
/// Aggregates data across multiple entities and sends it to the AI
/// for strategic analysis.
/// </summary>
public interface ICrossEntityAnalyticsService
{
    /// <summary>
    /// 🎓 Run a cross-entity analytics analysis for the specified category.
    ///
    /// The flow:
    ///   1. Validate the category string
    ///   2. Query EF Core to aggregate data for that category
    ///   3. Serialize aggregated data to JSON
    ///   4. Build the prompt from AnalyticsPromptTemplates
    ///   5. Call IAiService.CompleteAsync() with the analytics system prompt
    ///   6. Return the AI response + metadata (entity count, category)
    ///
    /// Categories: MATERIAL_CONSUMPTION, SUPPLIER_PERFORMANCE,
    ///             BUYER_PORTFOLIO, ORDER_PIPELINE, COST_ANALYSIS
    /// </summary>
    /// <param name="category">The analytics category to run (case-insensitive).</param>
    /// <param name="userQuery">Optional user question to focus the analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple of (AiCompletionResponse, entityCount) where entityCount is
    /// the number of entities included in the aggregated dataset.
    /// </returns>
    Task<(AiCompletionResponse Response, int EntityCount)> GetAnalyticsAsync(
        string category,
        string? userQuery = null,
        CancellationToken cancellationToken = default);
}
