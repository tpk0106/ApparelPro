namespace ApparelPro.AI.Configuration;

/// <summary>
/// Configuration settings for AI providers. Bound from appsettings.json section "AiSettings".
/// </summary>
public sealed class AiSettings
{
    public const string SectionName = "AiSettings";

    /// <summary>
    /// The active provider: "Anthropic" or "OpenAI".
    /// Switch between providers via config without code changes.
    /// </summary>
    public required string ActiveProvider { get; set; }

    /// <summary>
    /// Anthropic (Claude) configuration.
    /// </summary>
    public AnthropicSettings Anthropic { get; set; } = new();

    /// <summary>
    /// OpenAI (GPT) configuration.
    /// </summary>
    public OpenAiSettings OpenAI { get; set; } = new();

    /// <summary>
    /// Default max tokens for quick summaries if not specified per-request.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1024;

    /// <summary>
    /// Max tokens for deep analysis responses — higher budget for actionable insights.
    /// </summary>
    public int AnalysisMaxTokens { get; set; } = 2048;

    /// <summary>
    /// Default temperature if not specified per-request.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.3;

    /// <summary>
    /// Max tokens for chat responses (default 1500 — balanced for conversational replies).
    /// </summary>
    public int ChatMaxTokens { get; set; } = 1500;
}

public sealed class AnthropicSettings
{
    /// <summary>
    /// Anthropic API key. Store in User Secrets or Azure Key Vault — never in appsettings.json.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model identifier (e.g. "claude-sonnet-4-20250514").
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-20250514";
}

public sealed class OpenAiSettings
{
    /// <summary>
    /// OpenAI API key. Store in User Secrets or Azure Key Vault — never in appsettings.json.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model identifier (e.g. "gpt-4o").
    /// </summary>
    public string Model { get; set; } = "gpt-4o";
}
