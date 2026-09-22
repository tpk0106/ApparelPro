using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Core AI service abstraction — routes requests to the active (or specified) provider.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Generate a concise summary of an entity's data.
    /// </summary>
    Task<AiCompletionResponse> SummariseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a deep analysis of an entity's data.
    /// </summary>
    Task<AiCompletionResponse> AnalyseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a completion request using the default active provider.
    /// </summary>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a completion request using a specific provider override.
    /// Falls back to the default active provider if the specified one is not available.
    /// Used by voice chat to force OpenAI regardless of the configured active provider.
    /// </summary>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        string preferredProvider,
        CancellationToken cancellationToken = default);
}
