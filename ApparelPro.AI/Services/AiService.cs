using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;
using ApparelPro.AI.Prompts;

namespace ApparelPro.AI.Services;

/// <summary>
/// Core AI service that routes requests to the active provider.
/// Inject IAiService in your controllers — never reference this class directly.
///
/// Prompt tuning notes (2024-2026):
/// - Summarise MaxTokens now uses configurable DefaultMaxTokens (2500) for enriched summaries.
/// - Analyse uses configurable AnalysisMaxTokens (default 2500).
/// - Temperature lowered across modes for more factual, consistent output.
/// - Added provider override support for voice chat (always OpenAI).
/// </summary>
public sealed class AiService : IAiService
{
    private readonly Dictionary<string, IAiProvider> _providers;
    private readonly AiSettings _settings;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IEnumerable<IAiProvider> providers,
        IOptions<AiSettings> settings,
        ILogger<AiService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _providers = providers.ToDictionary(
            p => p.ProviderName,
            p => p,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<AiCompletionResponse> SummariseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default)
    {
        var template = PromptTemplates.GetSummariseTemplate(entityType);
        var userQuerySection = string.IsNullOrWhiteSpace(userQuery)
            ? string.Empty
            : $"Additionally, answer this question: {userQuery}";

        // Resolve the prompt template based on entity type
        string userMessage;
        if (entityType.Equals("STYLE", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("PURCHASEORDER", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("PO", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("SUPPLIER", StringComparison.OrdinalIgnoreCase))
        {
            userMessage = string.Format(template, entityData, userQuerySection);
        }
        else
        {
            userMessage = string.Format(template, entityType, entityData, userQuerySection);
        }

        var request = new AiCompletionRequest
        {
            SystemPrompt = PromptTemplates.SummariseSystem,
            UserMessage = userMessage,
            MaxTokens = _settings.DefaultMaxTokens,  // Now configurable — default 2500 for enriched summaries
            Temperature = 0.25      // Aligned with analyse temperature for consistent output
        };

        _logger.LogInformation(
            "Summarising {EntityType} entity via {Provider}",
            entityType, _settings.ActiveProvider);

        return await CompleteAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AiCompletionResponse> AnalyseEntityAsync(
        string entityType,
        string entityData,
        string? userQuery = null,
        CancellationToken cancellationToken = default)
    {
        var template = PromptTemplates.GetAnalyseTemplate(entityType);
        var userQuerySection = string.IsNullOrWhiteSpace(userQuery)
            ? string.Empty
            : $"Additionally, focus on this question: {userQuery}";

        // Resolve the prompt template based on entity type
        string userMessage;
        if (entityType.Equals("STYLE", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("PURCHASEORDER", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("PO", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("SUPPLIER", StringComparison.OrdinalIgnoreCase))
        {
            userMessage = string.Format(template, entityData, userQuerySection);
        }
        else
        {
            userMessage = string.Format(template, entityType, entityData, userQuerySection);
        }

        var request = new AiCompletionRequest
        {
            SystemPrompt = PromptTemplates.AnalyseSystem,
            UserMessage = userMessage,
            MaxTokens = _settings.AnalysisMaxTokens,  // 2500 default — configurable
            Temperature = 0.25  // Lowered from 0.3 — factual analysis benefits from consistency
        };

        _logger.LogInformation(
            "Analysing {EntityType} entity via {Provider} (max tokens: {MaxTokens})",
            entityType, _settings.ActiveProvider, _settings.AnalysisMaxTokens);

        return await CompleteAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return await CompleteWithProviderAsync(
            request, _settings.ActiveProvider, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        string preferredProvider,
        CancellationToken cancellationToken = default)
    {
        // Use the preferred provider if registered; otherwise fall back to active
        var providerName = _providers.ContainsKey(preferredProvider)
            ? preferredProvider
            : _settings.ActiveProvider;

        if (providerName != preferredProvider)
        {
            _logger.LogWarning(
                "Preferred provider '{PreferredProvider}' is not registered. " +
                "Falling back to active provider '{ActiveProvider}'.",
                preferredProvider, _settings.ActiveProvider);
        }

        return await CompleteWithProviderAsync(request, providerName, cancellationToken);
    }

    // ─── Private: resolve provider and execute ───────────

    private async Task<AiCompletionResponse> CompleteWithProviderAsync(
        AiCompletionRequest request,
        string providerName,
        CancellationToken cancellationToken)
    {
        if (!_providers.TryGetValue(providerName, out var provider))
        {
            var available = string.Join(", ", _providers.Keys);
            throw new InvalidOperationException(
                $"AI provider '{providerName}' is not registered. " +
                $"Available providers: {available}. " +
                $"Check the 'AiSettings:ActiveProvider' configuration.");
        }

        try
        {
            return await provider.CompleteAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AI completion failed via {Provider}. Model: {Model}",
                provider.ProviderName, providerName);
            throw;
        }
    }
}
