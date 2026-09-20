namespace ApparelPro.WebApi.Controllers;

/// <summary>
/// Response model for the AI analyse endpoint.
/// Contains the full analysis plus token usage for cost tracking.
/// </summary>
public sealed class AiAnalyseAPIModel_Response
{
    /// <summary>
    /// The AI-generated deep analysis content (structured with headings,
    /// risk flags, and recommendations).
    /// </summary>
    public required string Analysis { get; set; }

    /// <summary>
    /// The entity type that was analysed.
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// The entity key that was analysed.
    /// </summary>
    public required string EntityKey { get; set; }

    /// <summary>
    /// Which AI provider generated this analysis.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// The model identifier used (e.g. "claude-sonnet-4-20250514").
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Input tokens consumed (for cost tracking).
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Output tokens generated (for cost tracking).
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Total tokens consumed (input + output).
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// When this analysis was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}
