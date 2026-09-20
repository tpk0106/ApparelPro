using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Abstraction over AI providers (Anthropic, OpenAI).
/// Implement this interface to add a new provider.
/// </summary>
public interface IAiProvider
{
    /// <summary>
    /// Unique provider name (e.g. "Anthropic", "OpenAI").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Send a completion request to the AI provider.
    /// </summary>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}
