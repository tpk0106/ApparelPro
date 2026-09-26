namespace ApparelPro.AI.Models;

/// <summary>
/// API-level response model for the summarise endpoint.
/// </summary>
public sealed class AiSummariseAPIModel_Response
{
    /// <summary>
    /// The AI-generated summary text.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// The entity type that was summarised.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// The entity key that was summarised.
    /// </summary>
    public required string EntityKey { get; init; }

    /// <summary>
    /// Which AI provider generated the summary.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// The model identifier used (e.g. "claude-sonnet-4-20250514").
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Input tokens consumed (for cost tracking).
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Output tokens generated (for cost tracking).
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Total tokens consumed (input + output).
    /// </summary>
    public int TotalTokens { get; init; }

    /// <summary>
    /// Estimated cost in USD for this completion.
    /// </summary>
    public decimal EstimatedCost { get; init; }

    /// <summary>
    /// Timestamp of when the summary was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}
