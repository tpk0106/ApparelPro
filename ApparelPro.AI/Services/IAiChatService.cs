using ApparelPro.Data.Models.AI;

namespace ApparelPro.AI.Services;

/// <summary>
/// Service contract for multi-turn AI chat session management.
/// Handles session lifecycle, message persistence, and AI completion with conversation history.
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Sends a message in a chat session. If sessionId is null, creates a new session
    /// bound to the specified entity and returns the AI response with session metadata.
    /// </summary>
    Task<AiChatResponse> SendMessageAsync(
        string userId,
        Guid? sessionId,
        string? entityType,
        string? entityKey,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists chat sessions for a user, ordered by most recent activity.
    /// </summary>
    Task<AiChatSessionListResult> GetSessionsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single session with its full message history.
    /// Returns null if the session doesn't exist or belongs to another user.
    /// </summary>
    Task<AiChatSession?> GetSessionWithMessagesAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a chat session. Returns true if found and deleted, false if not found.
    /// </summary>
    Task<bool> DeleteSessionAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}

// ─── Response models ─────────────────────────────────────

public class AiChatResponse
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
    public DateTime CreatedAt { get; set; }
    public bool IsNewSession { get; set; }
}

public class AiChatSessionListResult
{
    public IList<AiChatSessionSummary> Sessions { get; set; } = new List<AiChatSessionSummary>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AiChatSessionSummary
{
    public Guid SessionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
}
