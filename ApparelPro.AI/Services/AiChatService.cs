using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Models;
using ApparelPro.AI.Prompts;
using ApparelPro.Data;
using ApparelPro.Data.Models.AI;
using apparelPro.BusinessLogic.Services;

namespace ApparelPro.AI.Services;

/// <summary>
/// Multi-turn AI chat service.
/// Manages session lifecycle, message persistence, and AI completion with conversation history.
/// Supports provider override for voice chat (always OpenAI).
/// </summary>
public sealed class AiChatService : IAiChatService
{
    private readonly ApparelProDbContext _apparelProDbContext;
    private readonly IAiService _aiService;
    private readonly IStyleDetailsService _styleDetailsService;
    private readonly IMaterialConsumptionService _materialConsumptionService;
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISupplierService _supplierService;
    private readonly IBuyerService _buyerService;
    private readonly AiSettings _settings;
    private readonly ILogger<AiChatService> _logger;

    /// <summary>
    /// Maximum number of past messages to include in conversation context.
    /// Controls token cost — older messages are trimmed.
    /// </summary>
    private const int MaxHistoryMessages = 20;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AiChatService(
        ApparelProDbContext apparelProDbContext,
        IAiService aiService,
        IStyleDetailsService styleDetailsService,
        IMaterialConsumptionService materialConsumptionService,
        IPurchaseOrderService purchaseOrderService,
        ISupplierService supplierService,
        IBuyerService buyerService,
        IOptions<AiSettings> settings,
        ILogger<AiChatService> logger)
    {
        _apparelProDbContext = apparelProDbContext;
        _aiService = aiService;
        _styleDetailsService = styleDetailsService;
        _materialConsumptionService = materialConsumptionService;
        _purchaseOrderService = purchaseOrderService;
        _supplierService = supplierService;
        _buyerService = buyerService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiChatResponse> SendMessageAsync(
        string userId,
        Guid? sessionId,
        string? entityType,
        string? entityKey,
        string message,
        string? preferredProvider = null,
        CancellationToken cancellationToken = default)
    {
        AiChatSession session;
        bool isNewSession;

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            // ─── Continue existing session ───────────────────
            // AsNoTracking: we only need the history for prompt context.
            // This prevents change-tracker conflicts when rapid voice
            // requests hit the same session concurrently.
            session = await _apparelProDbContext.AiChatSessions
                .AsNoTracking()
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(
                    s => s.SessionId == sessionId.Value
                         && s.UserId == userId
                         && s.IsActive,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Chat session not found: {sessionId.Value}");

            isNewSession = false;
        }
        else
        {
            // ─── Create new session ──────────────────────────
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityKey))
                throw new ArgumentException(
                    "EntityType and EntityKey are required when starting a new chat session.");

            var entityData = await ResolveEntityDataAsync(
                entityType.Trim(), entityKey.Trim(), cancellationToken);

            // ── Detach read-only entities loaded during resolution ──
            // ResolveEntityDataAsync loads Style, Buyer, Ledger etc. as tracked
            // entities. If they stay tracked, SaveChangesAsync tries to persist
            // them and hits DbUpdateConcurrencyException. We only need the
            // serialised JSON snapshot — detach everything that isn't an AI entity.
            foreach (var entry in _apparelProDbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.Entity is not AiChatSession and not AiChatMessage)
                    entry.State = EntityState.Detached;
            }

            session = new AiChatSession
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                EntityType = entityType.Trim().ToUpperInvariant(),
                EntityKey = entityKey.Trim(),
                Title = ChatPromptTemplates.GenerateSessionTitle(
                    entityType.Trim(), entityKey.Trim(), message),
                EntityDataSnapshot = entityData,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true,
                Messages = new List<AiChatMessage>()
            };

            _apparelProDbContext.AiChatSessions.Add(session);
            isNewSession = true;

            _logger.LogInformation(
                "Created new chat session {SessionId} for {EntityType}/{EntityKey} by user {UserId}",
                session.SessionId, session.EntityType, session.EntityKey, userId);
        }

        // ─── Persist the user message ────────────────────────
        var userMessage = new AiChatMessage
        {
            MessageId = Guid.NewGuid(),
            SessionId = session.SessionId,
            Role = "user",
            Content = message,
            TokensUsed = 0,
            CreatedAt = DateTime.UtcNow
        };
        // Add to in-memory collection for conversation history building
        session.Messages.Add(userMessage);

        // For continuing sessions (AsNoTracking), register directly with
        // the DbContext so the INSERT goes through SaveChangesAsync.
        if (!isNewSession)
            _apparelProDbContext.AiChatMessages.Add(userMessage);

        // ─── Build conversation context ──────────────────────
        var systemPrompt = ChatPromptTemplates.BuildChatSystemPrompt(
            session.EntityType, session.EntityDataSnapshot ?? "{}");

        // Voice mode: append spoken-language formatting rules so the AI
        // responds in natural speech rather than markdown/lists.
        if (!string.IsNullOrWhiteSpace(preferredProvider))
        {
            systemPrompt += ChatPromptTemplates.VoiceModeAddendum;
        }

        // Get recent history (excluding the just-added user message)
        var history = session.Messages
            .Where(m => m.MessageId != userMessage.MessageId)
            .OrderBy(m => m.CreatedAt)
            .TakeLast(MaxHistoryMessages)
            .Select(m => (m.Role, m.Content))
            .ToList();

        var conversationMessage = ChatPromptTemplates.BuildConversationMessage(
            history, message);

        // ─── Call AI provider ────────────────────────────────
        var request = new AiCompletionRequest
        {
            SystemPrompt = systemPrompt,
            UserMessage = conversationMessage,
            MaxTokens = _settings.ChatMaxTokens,   // configurable, default 1500
            Temperature = 0.4                       // balanced for chat
        };

        // Use preferred provider if specified (e.g. "OpenAI" for voice mode)
        var aiResponse = !string.IsNullOrWhiteSpace(preferredProvider)
            ? await _aiService.CompleteAsync(request, preferredProvider, cancellationToken)
            : await _aiService.CompleteAsync(request, cancellationToken);

        // ─── Persist the assistant response ──────────────────
        var assistantMessage = new AiChatMessage
        {
            MessageId = Guid.NewGuid(),
            SessionId = session.SessionId,
            Role = "assistant",
            Content = aiResponse.Content,
            TokensUsed = aiResponse.TotalTokens,
            CreatedAt = DateTime.UtcNow
        };
        // For continuing sessions, register directly with the DbContext.
        // For new sessions, the navigation property keeps it tracked.
        if (isNewSession)
        {
            session.Messages.Add(assistantMessage);
            session.LastMessageAt = DateTime.UtcNow;
        }
        else
        {
            session.Messages.Add(assistantMessage); // in-memory only (AsNoTracking)
            _apparelProDbContext.AiChatMessages.Add(assistantMessage);
        }

        // ── Persist ─────────────────────────────────────────────
        // New session: single save inserts session + both messages.
        // Continuing session: save inserts only the two new messages
        // (session is untracked), then ExecuteUpdateAsync atomically
        // bumps LastMessageAt — no change-tracker conflict possible.
        await _apparelProDbContext.SaveChangesAsync(cancellationToken);

        if (!isNewSession)
        {
            await _apparelProDbContext.AiChatSessions
                .Where(s => s.SessionId == session.SessionId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        s => s.LastMessageAt, DateTime.UtcNow),
                    cancellationToken);
        }

        _logger.LogInformation(
            "Chat message processed for session {SessionId}. Provider: {Provider}, Tokens: {Tokens}",
            session.SessionId, aiResponse.Provider, aiResponse.TotalTokens);

        return new AiChatResponse
        {
            SessionId = session.SessionId,
            SessionTitle = session.Title ?? $"{session.EntityType}: {session.EntityKey}",
            MessageId = assistantMessage.MessageId,
            Reply = aiResponse.Content,
            Provider = aiResponse.Provider,
            Model = aiResponse.Model,
            InputTokens = aiResponse.InputTokens,
            OutputTokens = aiResponse.OutputTokens,
            TotalTokens = aiResponse.TotalTokens,
            EstimatedCost = aiResponse.EstimatedCost,
            CreatedAt = assistantMessage.CreatedAt,
            IsNewSession = isNewSession
        };
    }

    /// <inheritdoc />
    public async Task<AiChatSessionListResult> GetSessionsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _apparelProDbContext.AiChatSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastMessageAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var sessions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AiChatSessionSummary
            {
                SessionId = s.SessionId,
                EntityType = s.EntityType,
                EntityKey = s.EntityKey,
                Title = s.Title,
                MessageCount = s.Messages.Count,
                CreatedAt = s.CreatedAt,
                LastMessageAt = s.LastMessageAt
            })
            .ToListAsync(cancellationToken);

        return new AiChatSessionListResult
        {
            Sessions = sessions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    /// <inheritdoc />
    public async Task<AiChatSession?> GetSessionWithMessagesAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _apparelProDbContext.AiChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(
                s => s.SessionId == sessionId
                     && s.UserId == userId
                     && s.IsActive,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSessionAsync(
        string userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _apparelProDbContext.AiChatSessions
            .FirstOrDefaultAsync(
                s => s.SessionId == sessionId
                     && s.UserId == userId
                     && s.IsActive,
                cancellationToken);

        if (session == null)
            return false;

        // Soft-delete
        session.IsActive = false;
        await _apparelProDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Soft-deleted chat session {SessionId} for user {UserId}",
            sessionId, userId);

        return true;
    }

    // ─── Private: entity resolution ──────────────────────────

    /// <summary>
    /// Resolves the entity key into serialised JSON data.
    /// Same logic as AiController.ResolveEntityDataAsync but accessible from the service layer.
    /// </summary>
    private async Task<string> ResolveEntityDataAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken)
    {
        switch (entityType.ToUpperInvariant())
        {
            case "STYLE":
            {
                var parts = entityKey.Split('/');
                if (parts.Length != 4 ||
                    !int.TryParse(parts[0], out var buyerCode) ||
                    !int.TryParse(parts[2], out var typeCode))
                {
                    throw new ArgumentException(
                        "Style EntityKey must be 'buyerCode/order/typeCode/styleCode' " +
                        "(e.g. '1/ORD001/2/ST001').");
                }

                var style = await _styleDetailsService
                    .GetStyleDetailsByBuyerOrderTypeStyleAsync(
                        buyerCode, parts[1], typeCode, parts[3]);

                if (style == null)
                    throw new KeyNotFoundException($"Style not found: {entityKey}");

                // Enrich with buyer name
                var buyer = await _buyerService.GetBuyerByBuyerCodeAsync(buyerCode);

                var ledger = await _materialConsumptionService
                    .GetLedgerEntriesByStyleAsync(
                        buyerCode, parts[1], typeCode, parts[3]);

                var combined = new
                {
                    StyleDetails = style,
                    BuyerName = buyer?.Name,
                    MaterialConsumptionLedger = ledger,
                    ConsumptionLineCount = ledger?.Count ?? 0
                };

                return JsonSerializer.Serialize(combined, JsonOptions);
            }

            case "PURCHASEORDER":
            case "PO":
            {
                var parts = entityKey.Split('/');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out var buyerCode))
                {
                    throw new ArgumentException(
                        "PurchaseOrder EntityKey must be 'buyerCode/order' " +
                        "(e.g. '1/ORD001').");
                }

                var po = await _purchaseOrderService
                    .GetPurchaseOrderByBuyerAndOrderAsync(buyerCode, parts[1]);

                if (po == null)
                    throw new KeyNotFoundException($"Purchase Order not found: {entityKey}");

                return JsonSerializer.Serialize(po, JsonOptions);
            }

            case "SUPPLIER":
            {
                if (!int.TryParse(entityKey, out var supplierCode))
                {
                    throw new ArgumentException(
                        "Supplier EntityKey must be a numeric supplier code (e.g. '101').");
                }

                var supplier = await _supplierService
                    .GetSupplierBySupplierCodeAsync(supplierCode);

                if (supplier == null)
                    throw new KeyNotFoundException($"Supplier not found: {entityKey}");

                return JsonSerializer.Serialize(supplier, JsonOptions);
            }

            case "GENERAL":
            case "VOICE":
            {
                // Voice / general-purpose chat — no entity resolution needed.
                // The AI will respond as a general ApparelPro assistant.
                var generalData = new
                {
                    Mode = entityType.ToUpperInvariant() == "VOICE" ? "Voice" : "General",
                    Note = "General conversation — no specific entity context."
                };
                return JsonSerializer.Serialize(generalData, JsonOptions);
            }

            default:
                throw new ArgumentException(
                    $"Unsupported entity type: '{entityType}'. " +
                    "Supported types: Style, PurchaseOrder, Supplier, General, Voice.");
        }
    }
}
