namespace ApparelPro.AI.Models;

/// <summary>
/// Response from a chat message send operation.
/// Returned by AiChatService.SendMessageAsync.
/// </summary>
public sealed class AiChatResponse
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
/// Paginated result of chat session listings.
/// Returned by AiChatService.GetSessionsAsync.
/// </summary>
public sealed class AiChatSessionListResult
{
    public IList<AiChatSessionSummary> Sessions { get; set; } = new List<AiChatSessionSummary>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Summary of a chat session for list views.
/// </summary>
public sealed class AiChatSessionSummary
{
    public Guid SessionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int MessageCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
}
