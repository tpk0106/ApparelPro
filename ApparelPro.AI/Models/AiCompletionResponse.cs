namespace ApparelPro.AI.Models;

/// <summary>
/// Represents the response from an AI completion.
/// </summary>
public sealed class AiCompletionResponse
{
    /// <summary>
    /// The generated text content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Which provider generated this response.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// The model identifier used (e.g. "claude-sonnet-4-20250514", "gpt-4o").
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Total tokens consumed (input + output) for cost tracking.
    /// </summary>
    public int TotalTokens { get; init; }

    /// <summary>
    /// Input tokens consumed.
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Output tokens generated.
    /// </summary>
    public int OutputTokens { get; init; }
}
