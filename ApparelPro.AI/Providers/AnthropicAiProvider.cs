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
///
/// 🎓 STREAMING MODE (Oct 2026 fix):
/// Previously used the non-streaming Messages API (POST → wait → full JSON response).
/// This caused "ResponseEnded" errors for large predictions because:
///   - The AI takes 30-90 seconds to generate a 3500-5000 token prescriptive response
///   - During generation, NO data flows back on the TCP connection
///   - Network devices (routers, firewalls, ISPs) have idle-connection timeouts (often 60s)
///   - After 60s of silence, the network kills the "idle" connection → ResponseEnded
///
/// The fix: use Anthropic's Server-Sent Events (SSE) streaming endpoint.
///   - Add "stream": true to the request body
///   - Anthropic sends back a stream of events: content_block_delta, message_delta, etc.
///   - Data flows continuously, so no network device sees an idle connection
///   - We accumulate the text chunks and parse the final usage from message_delta
///
/// This is transparent to callers — the method signature and return type are unchanged.
/// The only difference is HOW we receive the data (streaming vs. blocking).
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

        // 🎓 HttpClient timeout set to 5 minutes — now a safety net rather than
        //    the primary timeout mechanism. With streaming, data arrives continuously,
        //    so this timeout should never fire unless something is truly broken.
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
            "Sending streaming request to Anthropic ({Model}), max tokens: {MaxTokens}",
            _settings.Anthropic.Model, maxTokens);

        // 🎓 "stream": true tells Anthropic to return Server-Sent Events (SSE)
        //    instead of a single JSON response. The response arrives as a series
        //    of "event: xxx\ndata: {...}\n\n" lines.
        var body = new
        {
            model = _settings.Anthropic.Model,
            max_tokens = maxTokens,
            temperature,
            stream = true,
            system = request.SystemPrompt ?? string.Empty,
            messages = new[]
            {
                new { role = "user", content = request.UserMessage }
            }
        };

        var json = JsonSerializer.Serialize(body);

        _logger.LogInformation(
            "Anthropic request payload size: {SizeKB:F1} KB ({SizeBytes} bytes)",
            json.Length / 1024.0, json.Length);

        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        // 🎓 HttpCompletionOption.ResponseHeadersRead is CRITICAL for streaming:
        //    - Default behavior: .NET waits for the ENTIRE response body before returning
        //    - ResponseHeadersRead: .NET returns as soon as headers arrive, letting us
        //      read the body as a stream — exactly what SSE needs
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = httpContent
        };

        using var response = await _http.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        // 🎓 Non-2xx errors still come as regular JSON (not SSE), so handle them
        //    the same way as before
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Anthropic API error {Status}: {Body}",
                response.StatusCode, errorBody);
            throw new HttpRequestException(
                $"Anthropic API returned {response.StatusCode}: {errorBody}");
        }

        // 🎓 Read the SSE stream and accumulate the response text + usage stats
        return await ReadStreamAsync(response, cancellationToken);
    }

    /// <summary>
    /// Reads Anthropic's SSE stream and assembles the final response.
    ///
    /// 🎓 SSE EVENT FORMAT (Anthropic Messages API):
    /// Each event is two lines followed by a blank line:
    ///   event: content_block_delta
    ///   data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}
    ///
    /// Key event types we care about:
    ///   - content_block_delta → contains a text chunk in delta.text
    ///   - message_delta       → contains stop_reason and output token usage
    ///   - message_start       → contains input token usage in message.usage
    ///   - error               → API-level error during generation
    ///
    /// We ignore: message_start (except for input tokens), content_block_start,
    /// content_block_stop, message_stop, ping
    /// </summary>
    private async Task<AiCompletionResponse> ReadStreamAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var textBuilder = new StringBuilder();
        var inputTokens = 0;
        var outputTokens = 0;

        // 🎓 ReadAsStreamAsync gives us a raw byte stream over the TCP connection.
        //    We wrap it in a StreamReader to read line-by-line (SSE is line-delimited).
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? currentEvent = null;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line == null)
                break;

            // 🎓 SSE protocol:
            //    "event: <name>"  → sets the event type for the next data line
            //    "data: <json>"   → the payload
            //    ""               → end of this event (blank line separator)
            if (line.StartsWith("event: ", StringComparison.Ordinal))
            {
                currentEvent = line.Substring(7).Trim();
            }
            else if (line.StartsWith("data: ", StringComparison.Ordinal))
            {
                var dataJson = line.Substring(6);

                try
                {
                    using var doc = JsonDocument.Parse(dataJson);
                    var root = doc.RootElement;

                    switch (currentEvent)
                    {
                        // 🎓 message_start carries the input token count in
                        //    message.usage.input_tokens — this is the only place
                        //    we can get input tokens in streaming mode
                        case "message_start":
                            if (root.TryGetProperty("message", out var msg) &&
                                msg.TryGetProperty("usage", out var startUsage) &&
                                startUsage.TryGetProperty("input_tokens", out var inTok))
                            {
                                inputTokens = inTok.GetInt32();
                            }
                            break;

                        // 🎓 content_block_delta carries each text chunk — the core
                        //    of the streamed response. We append each chunk to build
                        //    the full text.
                        case "content_block_delta":
                            if (root.TryGetProperty("delta", out var delta) &&
                                delta.TryGetProperty("text", out var textChunk))
                            {
                                textBuilder.Append(textChunk.GetString());
                            }
                            break;

                        // 🎓 message_delta carries output_tokens — sent once at the
                        //    end when generation is complete
                        case "message_delta":
                            if (root.TryGetProperty("usage", out var endUsage) &&
                                endUsage.TryGetProperty("output_tokens", out var outTok))
                            {
                                outputTokens = outTok.GetInt32();
                            }
                            break;

                        // 🎓 error event — Anthropic encountered an error mid-stream
                        //    (e.g., rate limit hit during generation). Log and throw.
                        case "error":
                            var errorMsg = root.TryGetProperty("error", out var errObj) &&
                                           errObj.TryGetProperty("message", out var errText)
                                ? errText.GetString()
                                : "Unknown streaming error";
                            _logger.LogError("Anthropic streaming error: {Error}", errorMsg);
                            throw new HttpRequestException(
                                $"Anthropic streaming error: {errorMsg}");
                    }
                }
                catch (JsonException ex)
                {
                    // 🎓 Some SSE lines might not be valid JSON (e.g., "[DONE]" sentinel).
                    //    Safe to skip — the important data has already been accumulated.
                    _logger.LogDebug("Skipped non-JSON SSE data: {Data} ({Error})",
                        dataJson, ex.Message);
                }
            }
            // 🎓 Blank lines and other lines (comments starting with ":") are ignored
        }

        var text = textBuilder.ToString();

        _logger.LogInformation(
            "Anthropic streaming response complete. Input: {InputTokens}, Output: {OutputTokens}, Text length: {TextLen}",
            inputTokens, outputTokens, text.Length);

        // 🎓 Claude Sonnet 4.6 pricing: $3/MTok input, $15/MTok output
        //    (same as Sonnet 4 — check Anthropic pricing page if model changes)
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
