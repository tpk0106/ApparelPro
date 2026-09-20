namespace ApparelPro.AI.Models;

/// <summary>
/// Represents a request to generate an AI completion.
/// </summary>
public sealed class AiCompletionRequest
{
    /// <summary>
    /// The system prompt that sets the AI's behaviour and context.
    /// </summary>
    public required string SystemPrompt { get; init; }

    /// <summary>
    /// The user message or query to process.
    /// </summary>
    public required string UserMessage { get; init; }

    /// <summary>
    /// Optional maximum tokens for the response. Defaults to provider setting if null.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Optional temperature (0.0–1.0). Lower = more deterministic.
    /// </summary>
    public double? Temperature { get; init; }
}
