using System.ComponentModel.DataAnnotations;

namespace ApparelPro.WebApi.APIModels.AI;

// ─── Request Models ──────────────────────────────────────

/// <summary>
/// Request body for sending a chat message.
/// If SessionId is null/empty, a new session is created (EntityType + EntityKey required).
/// If SessionId is provided, continues the existing conversation.
/// </summary>
public class AiChatSendMessageAPIModel
{
    /// <summary>
    /// Existing session ID to continue. Null/empty to start a new session.
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Entity type (required for new sessions). E.g. "STYLE", "PURCHASEORDER", "SUPPLIER".
    /// </summary>
    [MaxLength(50)]
    public string? EntityType { get; set; }

    /// <summary>
    /// Entity key (required for new sessions). E.g. "1/ORD001/2/ST001".
    /// </summary>
    [MaxLength(200)]
    public string? EntityKey { get; set; }

    /// <summary>
    /// The user's message text.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional AI provider override. When set (e.g. "OpenAI"), forces the backend
    /// to use that provider instead of the configured default.
    /// Used by voice chat to always route through OpenAI.
    /// </summary>
    [MaxLength(50)]
    public string? PreferredProvider { get; set; }
}

// ─── Response Models ─────────────────────────────────────

/// <summary>
/// Response returned after sending a chat message.
/// </summary>
public class AiChatMessageAPIModel_Response
{
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public Guid MessageId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsNewSession { get; set; }
}

/// <summary>
/// Summary of a chat session for list views.
/// </summary>
public class AiChatSessionAPIModel_Response
{
    public Guid SessionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int MessageCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
}

/// <summary>
/// Full session detail with all messages.
/// </summary>
public class AiChatSessionDetailAPIModel_Response
{
    public Guid SessionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
    public IList<AiChatMessageItemAPIModel> Messages { get; set; } = new List<AiChatMessageItemAPIModel>();
}

/// <summary>
/// Individual message within a session.
/// </summary>
public class AiChatMessageItemAPIModel
{
    public Guid MessageId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
