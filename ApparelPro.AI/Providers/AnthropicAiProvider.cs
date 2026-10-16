using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;

namespace ApparelPro.AI.Providers;

/// <summary>
/// Anthropic Claude provider — calls the Messages API directly via HttpClient,
/// so the build never breaks when the third-party SDK changes its surface.
/// </summary>
public sealed class AnthropicAiProvider : IAiProvider
{
    private readonly HttpClient _http;
    private readonly AiSettings _settings;
    private readonly ILogger<AnthropicAiProvider> _logger;

    public string ProviderName => "Anthropic";

    public AnthropicAiProvider(
        IOptions<AiSettings> settings,
        ILogger<AnthropicAiProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // 🎓 HttpClient timeout set to 5 minutes to accommodate large
        //    cross-entity analytics prompts. The default 100s is often too
        //    short when the request body contains 50+ styles' worth of
        //    aggregated JSON data — Anthropic may need extra time to read
        //    the full request and generate a response.
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com"),
            Timeout = TimeSpan.FromMinutes(5)
        };
        _http.DefaultRequestHeaders.Add("x-api-key", _settings.Anthropic.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var maxTokens = request.MaxTokens ?? _settings.DefaultMaxTokens;
        var temperature = request.Temperature ?? _settings.DefaultTemperature;

        _logger.LogInformation(
            "Sending completion request to Anthropic ({Model}), max tokens: {MaxTokens}",
            _settings.Anthropic.Model, maxTokens);

        var body = new
        {
            model = _settings.Anthropic.Model,
            max_tokens = maxTokens,
            temperature,
            system = request.SystemPrompt ?? string.Empty,
            messages = new[]
            {
                new { role = "user", content = request.UserMessage }
            }
        };

        var json = JsonSerializer.Serialize(body);

        // 🎓 Log the request payload size — helps diagnose "ResponseEnded" errors
        //    which occur when the request body is too large for Anthropic to accept.
        //    Typical safe limit: ~1MB. If this exceeds ~500KB, consider capping the
        //    data sent in the analytics aggregation queries.
        _logger.LogInformation(
            "Anthropic request payload size: {SizeKB:F1} KB ({SizeBytes} bytes)",
            json.Length / 1024.0, json.Length);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync("/v1/messages", content, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API error {Status}: {Body}",
                response.StatusCode, responseBody);
            throw new HttpRequestException(
                $"Anthropic API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        // Extract text from the first text content block
        var text = string.Empty;
        if (root.TryGetProperty("content", out var contentArr))
        {
            foreach (var block in contentArr.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var typeEl) &&
                    typeEl.GetString() == "text" &&
                    block.TryGetProperty("text", out var textEl))
                {
                    text = textEl.GetString() ?? string.Empty;
                    break;
                }
            }
        }

        var inputTokens = root.GetProperty("usage").GetProperty("input_tokens").GetInt32();
        var outputTokens = root.GetProperty("usage").GetProperty("output_tokens").GetInt32();

        _logger.LogInformation(
            "Anthropic response received. Input: {InputTokens}, Output: {OutputTokens}",
            inputTokens, outputTokens);

        // Claude Sonnet pricing: $3/MTok input, $15/MTok output
        var estimatedCost = (inputTokens / 1_000_000m * 3m)
                          + (outputTokens / 1_000_000m * 15m);

        return new AiCompletionResponse
        {
            Content = text,
            Provider = ProviderName,
            Model = _settings.Anthropic.Model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens,
            EstimatedCost = Math.Round(estimatedCost, 6)
        };
    }
}