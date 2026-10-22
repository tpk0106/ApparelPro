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
    /// Default max tokens for quick summaries if not specified per-request.
    /// Raised from 1024 → 1024 (unchanged) — summarise uses explicit 1000 in AiService.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1024;

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

    // ─── 🆕 Phase 2: Cross-Entity Analytics ────────────

    /// <summary>
    /// Max tokens for cross-entity analytics responses.
    ///
    /// 🎓 WHY 3500 (HIGHER THAN AnalysisMaxTokens)?
    /// Cross-entity analytics covers MULTIPLE entities at once — the AI needs
    /// more room to build:
    ///   - Executive Summary (2-3 sentences)
    ///   - Key Metrics (tables + calculations)
    ///   - Patterns & Trends (3-5 findings with magnitude)
    ///   - Outliers & Risks (flagged items with severity ratings)
    ///   - Strategic Recommendations (3-5 ranked actions)
    ///
    /// Single-entity analysis (2500 tokens) covers ONE style/order.
    /// Cross-entity analytics (3500 tokens) covers ALL styles/orders —
    /// the extra 1000 tokens are for the additional comparisons, rankings,
    /// and cross-cutting patterns that single-entity analysis doesn't need.
    ///
    /// Configurable via appsettings.json:
    ///   "AiSettings": { "AnalyticsMaxTokens": 3500 }
    /// </summary>
    public int AnalyticsMaxTokens { get; set; } = 3500;

    // ─── 🆕 Phase 5: Predictive & Prescriptive AI ──────

    /// <summary>
    /// Max tokens for predictive + prescriptive AI responses.
    ///
    /// 🎓 WHY 5000 (HIGHER THAN AnalyticsMaxTokens)?
    /// Prescriptive AI outputs are the most detailed in the system because
    /// they include ALL of the following for each prediction category:
    ///   - Prediction Summary (2-3 sentences)
    ///   - Data Signals table (trend indicators with direction arrows)
    ///   - Detailed Forecast (best/expected/worst scenarios with numbers)
    ///   - Risk Factors (rated probabilities)
    ///   - Prescriptive Action Plan with structured fields per action:
    ///     Action, Priority (P1-P4), Owner, Timeline, Causality Chain,
    ///     Expected Impact ($/%/units), First Step, Success Metric
    ///   - Decision Matrix comparing 2-3 strategic options with trade-offs
    ///
    /// The extra 1500 tokens over AnalyticsMaxTokens (3500) accommodate:
    ///   - Causality chains ("If X → Y because Z") for each action
    ///   - Decision matrices with trade-off comparisons
    ///   - Implementation steps with owner assignments
    ///   - Quantified impact estimates per recommendation
    ///
    /// Configurable via appsettings.json:
    ///   "AiSettings": { "PredictiveMaxTokens": 5000 }
    /// </summary>
    public int PredictiveMaxTokens { get; set; } = 5000;

    // ─── 🆕 Phase 3: Anomaly Detection ──────────────────

    /// <summary>
    /// How often (in minutes) the AnomalyDetectionJob background service
    /// runs a full anomaly scan across all active styles.
    ///
    /// 🎓 WHY 30 MINUTES DEFAULT?
    /// - Material consumption data doesn't change every second — it updates
    ///   when warehouse staff record fabric issues or goods receipts.
    /// - 30 minutes strikes a balance between timely detection and DB load.
    /// - Too frequent (5 min): unnecessary load, no new data to scan.
    /// - Too infrequent (4 hrs): anomalies sit undetected for half a shift.
    /// - Configurable via appsettings.json: "AiSettings": { "AnomalyScanIntervalMinutes": 30 }
    /// </summary>
    public int AnomalyScanIntervalMinutes { get; set; } = 30;
}

public sealed class AnthropicSettings
{
    /// <summary>
    /// Anthropic API key. Store in User Secrets or Azure Key Vault — never in appsettings.json.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model identifier.
    /// 🎓 "claude-sonnet-4-6" is the current Sonnet 4 alias (Oct 2026).
    /// The old dated ID "claude-sonnet-4-20250514" was deprecated by Anthropic.
    /// This default is overridden by appsettings.json → AiSettings:Anthropic:Model.
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-6";
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
