using System.ClientModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;

namespace ApparelPro.AI.Providers;

/// <summary>
/// OpenAI GPT provider implementation.
/// </summary>
public sealed class OpenAiProvider : IAiProvider
{
    private readonly ChatClient _chatClient;
    private readonly AiSettings _settings;
    private readonly ILogger<OpenAiProvider> _logger;

    public string ProviderName => "OpenAI";

    public OpenAiProvider(
        IOptions<AiSettings> settings,
        ILogger<OpenAiProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(_settings.OpenAI.ApiKey));
        _chatClient = openAiClient.GetChatClient(_settings.OpenAI.Model);
    }

    public async Task<AiCompletionResponse> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var maxTokens = request.MaxTokens ?? _settings.DefaultMaxTokens;
        var temperature = (float)(request.Temperature ?? _settings.DefaultTemperature);

        _logger.LogInformation(
            "Sending completion request to OpenAI ({Model}), max tokens: {MaxTokens}",
            _settings.OpenAI.Model, maxTokens);

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = maxTokens,
            Temperature = temperature
        };

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(request.SystemPrompt),
            ChatMessage.CreateUserMessage(request.UserMessage)
        };

        var response = await _chatClient.CompleteChatAsync(
            messages,
            options,
            cancellationToken);

        var completion = response.Value;
        var content = completion.Content
            .Where(c => c.Kind == ChatMessageContentPartKind.Text)
            .Select(c => c.Text)
            .FirstOrDefault() ?? string.Empty;

        var inputTokens = completion.Usage?.InputTokenCount ?? 0;
        var outputTokens = completion.Usage?.OutputTokenCount ?? 0;

        _logger.LogInformation(
            "OpenAI response received. Input: {InputTokens}, Output: {OutputTokens}",
            inputTokens, outputTokens);

        return new AiCompletionResponse
        {
            Content = content,
            Provider = ProviderName,
            Model = _settings.OpenAI.Model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens
        };
    }
}
