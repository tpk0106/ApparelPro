namespace ApparelPro.AI.Models;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 CROSS-ENTITY ANALYTICS — API RESPONSE MODEL
// Phase 2, Feature 1: The response body from POST /api/ai/analytics.
//
// 🎓 WHY INCLUDE TOKEN/COST FIELDS?
// Cross-entity analytics sends MUCH more data to the AI than single-entity
// endpoints (potentially 50+ styles' worth of aggregated data). Token usage
// will be significantly higher, so cost visibility is critical. The frontend
// can display these figures to help administrators monitor AI spend.
//
// 🎓 FIELD MAPPING FROM AiCompletionResponse:
//   response.Content       → Analysis
//   response.Provider      → Provider
//   response.Model         → Model
//   response.InputTokens   → InputTokens
//   response.OutputTokens  → OutputTokens
//   response.TotalTokens   → TotalTokens
//   response.EstimatedCost → EstimatedCost
//   DateTimeOffset.UtcNow  → GeneratedAt
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// API-level response model for cross-entity analytics.
/// Contains the AI-generated strategic analysis plus token usage for cost tracking.
/// </summary>
public sealed class CrossEntityAnalyticsAPIModel_Response
{
    /// <summary>
    /// 🎓 The AI-generated analysis content — structured with Executive Summary,
    /// Key Metrics, Patterns & Trends, Outliers & Risks, and Strategic Recommendations.
    /// This is Markdown-formatted text ready for rendering in the frontend.
    /// </summary>
    public required string Analysis { get; set; }

    /// <summary>
    /// The analytics category that was analysed (echoed back from the request).
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// 🎓 How many entities were included in the aggregated data sent to the AI.
    /// Helps the user understand the scope: "Analysis based on 47 styles" vs "3 styles".
    /// A low count might indicate missing data rather than few styles.
    /// </summary>
    public int EntityCount { get; set; }

    /// <summary>
    /// Which AI provider generated this analysis (e.g. "Anthropic", "OpenAI").
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// The model identifier used (e.g. "claude-sonnet-4-20250514").
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Input tokens consumed — expect higher than single-entity (aggregated data is large).
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Output tokens generated — analytics responses are typically 2000-3500 tokens.
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
    /// When this analysis was generated (UTC).
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}
