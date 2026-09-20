using System.ComponentModel.DataAnnotations;

namespace ApparelPro.Data.Models.AI;

/// <summary>
/// Represents a multi-turn AI chat conversation bound to a specific entity.
/// Each session maintains its own conversation history and entity context.
/// </summary>
public class AiChatSession
{
    public Guid SessionId { get; set; }

    /// <summary>
    /// The authenticated user who owns this session (from JWT NameIdentifier claim).
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The entity type this chat is about (e.g. "STYLE", "PURCHASEORDER", "SUPPLIER").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The entity key (e.g. "1/ORD001/2/ST001" for a Style).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EntityKey { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable title for the session. Auto-generated from the first message
    /// or the entity type/key if not explicitly set.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Snapshot of the entity data at session creation time, serialised as JSON.
    /// This ensures the AI context remains consistent even if the entity changes.
    /// </summary>
    public string? EntityDataSnapshot { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    /// <summary>
    /// Soft-delete flag. Inactive sessions are excluded from listings.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation ──────────────────────────────────────────
    public ICollection<AiChatMessage> Messages { get; set; } = new List<AiChatMessage>();
}
