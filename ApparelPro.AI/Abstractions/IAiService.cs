using ApparelPro.AI.Models;

namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Core AI service contract. Inject this in controllers — never reference AiService directly.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Quick summarisation of an entity (existing — ~600 words, low token cost).
    /// </summary>
    Task<AiCompletionResponse> SummariseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deep analysis of an entity with actionable insights, risk flags,
    /// and prioritised recommendations (~1,500 words, moderate token cost).
    /// </summary>
    Task<AiCompletionResponse> AnalyseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Raw completion pass-through for custom prompts.
    /// </summary>
    Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}
