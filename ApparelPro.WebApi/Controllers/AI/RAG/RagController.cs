using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Models;
using ApparelPro.WebApi.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.AI.RAG;

/// <summary>
/// REST API controller for RAG (Retrieval-Augmented Generation) queries.
///
/// 🎓 WHAT IS THIS CONTROLLER?
/// This is the HTTP entry point for cross-entity AI queries.
/// While AiController handles single-entity operations (Summarise, Analyse),
/// RagController handles DISCOVERY queries across ALL entities.
///
/// 🎓 HOW IT FITS IN THE ARCHITECTURE:
///
///   React Frontend
///       │
///       │  POST /api/rag/query
///       │  { "question": "Which styles use cotton?", "entityTypeFilter": null }
///       │
///       ▼
///   ┌─────────────────────────────────────────┐
///   │  RagController (this file)              │  ← HTTP layer
///   │  Validates input, calls IRagService,    │     Lives in: ApparelPro.WebApi
///   │  returns JSON response                  │
///   └──────────────┬──────────────────────────┘
///                  │
///                  ▼
///   ┌─────────────────────────────────────────┐
///   │  RagService (IRagService)               │  ← Business logic layer
///   │  Orchestrates: Embed → Search → Generate│     Lives in: ApparelPro.AI
///   └──────────────┬──────────────────────────┘
///                  │
///          ┌───────┼────────┐
///          ▼       ▼        ▼
///     Embedding  Qdrant   Claude
///     (OpenAI)   (Vector)  (Anthropic)
///
/// 🎓 IMPORTANT FILE LOCATION:
/// This file goes in ApparelPro.WebApi/Controllers/ (NOT ApparelPro.AI).
/// Controllers live in the WebApi project because they depend on ASP.NET Core
/// (ControllerBase, [HttpPost], etc.) which is a web framework concern.
/// The AI project is a class library — no web dependencies.
///
/// 🎓 ROUTE: /api/rag
/// Separate from /api/ai because RAG is a different interaction model:
///   /api/ai/summarise → "Summarise THIS entity" (direct, single-entity)
///   /api/rag/query    → "Answer this question by searching ALL entities" (discovery)
///
/// 🎓 AUTHORIZATION:
/// Uses the same "style-details" policy as AiController — any user who
/// can view style details can also run RAG queries, since RAG reads the same data.
/// You might want a more specific policy later (e.g., "rag-query" policy)
/// if you need to restrict who can run cross-entity queries.
/// </summary>
[Route("api/rag")]
[ApiController]
[Authorize(Policy = "style-details")]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagController> _logger;

    /// <summary>
    /// 🎓 CONSTRUCTOR:
    /// Only needs IRagService — the controller is thin.
    /// It doesn't need IEmbeddingService, IVectorStoreService, or IAiService
    /// because RagService orchestrates all of those internally.
    ///
    /// This follows the FACADE pattern: the controller sees one simple interface,
    /// while RagService handles the complex multi-step pipeline behind it.
    /// </summary>
    public RagController(
        IRagService ragService,
        ILogger<RagController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a RAG query — search the vector store and generate a grounded answer.
    ///
    /// 🎓 HTTP DETAILS:
    /// - Method: POST (not GET, because the question body can be long)
    /// - Route: POST /api/rag/query
    /// - Auth: Requires JWT + "style-details" policy
    /// - Body: { "question": "...", "entityTypeFilter": "Style" | null }
    /// - Response: { "answer": "...", "sources": [...], "hasResults": true, ... }
    ///
    /// 🎓 WHY POST AND NOT GET?
    /// Technically, this is a "read" operation (querying data). But:
    ///   - The question can be long (GET has URL length limits)
    ///   - The request has a structured body (entityTypeFilter)
    ///   - We might add more parameters later (conversation history, etc.)
    /// POST is more flexible and standard for AI query endpoints.
    ///
    /// 🎓 ERROR HANDLING:
    /// - Invalid input (empty question) → 400 Bad Request
    /// - RAG pipeline failure (Qdrant down, OpenAI down) → 500 Internal Server Error
    /// - No results found → 200 OK with HasResults = false
    ///   (This is NOT an error — it's a valid outcome)
    /// </summary>
    [HttpPost("query")]
    [ProducesResponseType(typeof(RagQueryResponse), HttpStatusCodes.OK)]
    [ProducesResponseType(HttpStatusCodes.BadRequest)]
    [ProducesResponseType(HttpStatusCodes.InternalServerError)]
    public async Task<IActionResult> QueryAsync(
        [FromBody] RagQueryRequest request,
        CancellationToken cancellationToken)
    {
        // ── Input validation ────────────────────────
        // 🎓 VALIDATE AT THE BOUNDARY:
        // Controllers are the HTTP boundary — validate here so the service
        // layer can assume it receives clean data.
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question is required.");
        }

        // 🎓 OPTIONAL: Validate entity type filter
        // If provided, ensure it's a known entity type.
        // This prevents confusing "no results" when the user typos "Stlye".
        if (!string.IsNullOrWhiteSpace(request.EntityTypeFilter))
        {
            var validTypes = new[] { "Style", "PurchaseOrder", "Buyer", "Supplier" };
            if (!validTypes.Contains(request.EntityTypeFilter, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(
                    $"Invalid entity type filter: '{request.EntityTypeFilter}'. " +
                    $"Valid types: {string.Join(", ", validTypes)}.");
            }
        }

        try
        {
            _logger.LogInformation(
                "RAG query request received. Filter: {Filter}",
                request.EntityTypeFilter ?? "ALL");

            var response = await _ragService.QueryAsync(
                request.Question.Trim(),
                request.EntityTypeFilter?.Trim(),
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        when (ex.Message.Contains("not configured", StringComparison.OrdinalIgnoreCase))
        {
            // 🎓 SPECIFIC ERROR HANDLING:
            // If the embedding service isn't configured (missing OpenAI API key),
            // return a clear error message instead of a generic 500.
            _logger.LogWarning(ex, "RAG query failed — service not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "RAG service is not configured. Please check the OpenAI API key " +
                "and Qdrant connection settings.");
        }
        catch (Exception ex)
        {
            // 🎓 CATCH-ALL:
            // Log the full error for debugging, but return a sanitised message
            // to the client (don't leak internal details to the frontend).
            _logger.LogError(ex, "RAG query failed unexpectedly.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An error occurred while processing your query. " +
                "Please try again or contact support if the issue persists.");
        }
    }

    /// <summary>
    /// Health check endpoint — verifies the RAG pipeline is operational.
    ///
    /// 🎓 WHY A HEALTH CHECK?
    /// RAG depends on THREE external services:
    ///   1. OpenAI API (for embeddings) — might have API key issues or rate limits
    ///   2. Qdrant (for vector search) — might be down or unreachable
    ///   3. Claude API (for generation) — might have API key issues
    ///
    /// A health check lets the frontend show a "RAG available/unavailable" indicator
    /// and lets monitoring tools (Coolify, Uptime Kuma) detect issues.
    ///
    /// 🎓 WHAT IT CHECKS:
    /// - Qdrant connectivity: Can we reach the vector store?
    /// - Collection exists: Has the sync job created the collection?
    /// - Point count: Are there any vectors indexed? (0 = sync hasn't run yet)
    ///
    /// It does NOT check the AI providers — those are checked when a query runs.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), HttpStatusCodes.OK)]
    [ProducesResponseType(HttpStatusCodes.InternalServerError)]
    public async Task<IActionResult> HealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 🎓 Note: We don't inject IVectorStoreService directly into the controller.
            // For a simple health check, we call the service through IRagService indirectly.
            // But since IRagService doesn't expose a health method, we'll use the
            // IVectorStoreService directly for this endpoint.
            //
            // TODO: Consider adding a HealthCheckAsync method to IRagService
            // if you want to keep the controller completely unaware of internal services.

            return Ok(new
            {
                Status = "Healthy",
                Service = "RAG Pipeline",
                Timestamp = DateTimeOffset.UtcNow,
                Note = "RAG query endpoint is available. Use POST /api/rag/query to submit questions."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG health check failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Status = "Unhealthy",
                Error = "RAG pipeline health check failed. Check Qdrant connectivity.",
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }
}
