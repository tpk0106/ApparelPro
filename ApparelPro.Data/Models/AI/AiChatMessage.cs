using System.ComponentModel.DataAnnotations;

namespace ApparelPro.Data.Models.AI;

/// <summary>
/// A single message in an AI chat conversation.
/// Role is either "user" (human) or "assistant" (AI response).
/// </summary>
public class AiChatMessage
{
    public Guid MessageId { get; set; }

    public Guid SessionId { get; set; }

    /// <summary>
    /// Message role: "user" for human messages, "assistant" for AI responses.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The message content (user question or AI response text).
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Total tokens consumed by this exchange (input + output for assistant messages).
    /// </summary>
    public int TokensUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    // ─── Navigation ──────────────────────────────────────────
    public AiChatSession Session { get; set; } = null!;
}
