namespace ApparelPro.AI.Configuration;

/// <summary>
/// Configuration settings for AI providers. Bound from appsettings.json section "AiSettings".
///
/// Prompt tuning notes (2024):
/// - DefaultMaxTokens raised to 1024 to accommodate field-aware summarise prompts.
/// - AnalysisMaxTokens raised to 2500 for 5-dimension analysis with calculations.
/// - ChatMaxTokens stays 1500 — chat should be concise.
/// - DefaultTemperature lowered to 0.25 for more consistent, factual responses.
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
    /// Default max tokens for summarise responses.
    /// Raised from 1024 → 2500 to accommodate the upgraded summarise prompt
    /// which now includes analysis, risk flags, and recommendations.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 2500;

    /// <summary>
    /// Max tokens for deep analysis responses.
    /// Raised from 2048 → 2500 to accommodate the expanded 5-dimension analysis format
    /// with inline calculations, risk flags, and recommendations.
    /// </summary>
    public int AnalysisMaxTokens { get; set; } = 2500;

    /// <summary>
    /// Default temperature if not specified per-request.
    /// Lowered from 0.3 → 0.25 for more consistent, factual responses.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.25;

    /// <summary>
    /// Max tokens for chat responses.
    /// Kept at 1500 — chat should be conversational and concise, not report-length.
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
