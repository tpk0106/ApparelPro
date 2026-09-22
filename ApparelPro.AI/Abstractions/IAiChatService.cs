using ApparelPro.AI.Models;
using ApparelPro.Data.Models.AI;

namespace ApparelPro.AI.Abstractions;

/// <summary>
/// Multi-turn AI chat service abstraction.
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Send a message in a chat session (new or existing).
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="sessionId">Existing session ID, or null to start a new session.</param>
    /// <param name="entityType">Required for new sessions: the entity type (e.g. "Style").</param>
    /// <param name="entityKey">Required for new sessions: the entity key.</param>
    /// <param name="message">The user's message.</param>
    /// <param name="preferredProvider">
    /// Optional: force a specific AI provider (e.g. "OpenAI" for voice mode).
    /// When null, uses the configured active provider.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<AiChatResponse> SendMessageAsync(
        string userId,
        Guid? sessionId,
        string? entityType,
        string? entityKey,
        string message,
        string? preferredProvider = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated list of chat sessions for a user.
    /// </summary>
    Task<AiChatSessionListResult> GetSessionsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single session with all its messages.
    /// </summary>
    Task<AiChatSession?> GetSessionWithMessagesAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-delete a chat session.
    /// </summary>
    Task<bool> DeleteSessionAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
