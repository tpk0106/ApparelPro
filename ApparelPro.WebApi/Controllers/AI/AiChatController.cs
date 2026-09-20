using System.Security.Claims;
using ApparelPro.AI.Services;
using ApparelPro.WebApi.Misc;
using ApparelPro.WebApi.APIModels.AI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    /// <summary>
    /// Phase 2: Multi-turn AI chat endpoints.
    /// Supports creating chat sessions bound to entities, sending follow-up messages,
    /// listing sessions, viewing history, and deleting sessions.
    /// </summary>
    [Route("api/ai/chat")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Policy = "style-details")]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        /// <summary>
        /// Send a message in a chat session.
        /// If SessionId is null, creates a new session bound to the specified entity.
        /// If SessionId is provided, continues the existing conversation with context.
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(AiChatMessageAPIModel_Response), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        [ProducesResponseType(HttpStatusCodes.InternalServerError)]
        public async Task<IActionResult> SendMessageAsync(
            [FromBody] AiChatSendMessageAPIModel request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required.");

            // New session requires entity type + key
            if ((!request.SessionId.HasValue || request.SessionId == Guid.Empty) &&
                (string.IsNullOrWhiteSpace(request.EntityType) ||
                 string.IsNullOrWhiteSpace(request.EntityKey)))
            {
                return BadRequest(
                    "EntityType and EntityKey are required when starting a new chat session.");
            }

          //  var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            // DEBUG: Log all claims to see what's actually in the token
            //var allClaims = HttpContext.User.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();
            //Console.WriteLine("=== TOKEN CLAIMS ===");
            //foreach (var claim in allClaims)
            //    Console.WriteLine(claim);
            //Console.WriteLine("====================");

            var userId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            //if (string.IsNullOrWhiteSpace(userId))

            //return Unauthorized();

            try
            {
                var response = await _aiChatService.SendMessageAsync(
                    userId,
                    request.SessionId,
                    request.EntityType,
                    request.EntityKey,
                    request.Message,
                    cancellationToken);

                var result = new AiChatMessageAPIModel_Response
                {
                    SessionId = response.SessionId,
                    SessionTitle = response.SessionTitle,
                    MessageId = response.MessageId,
                    Reply = response.Reply,
                    Provider = response.Provider,
                    Model = response.Model,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens,
                    TotalTokens = response.TotalTokens,
                    CreatedAt = response.CreatedAt,
                    IsNewSession = response.IsNewSession
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// List the authenticated user's chat sessions, ordered by most recent activity.
        /// </summary>
        [HttpGet("sessions")]
        [ProducesResponseType(typeof(PaginatedAiChatSessionsAPIModel_Response), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSessionsAsync(
            [FromQuery] int pageSize = 20,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _aiChatService.GetSessionsAsync(
                userId, pageNumber, pageSize, cancellationToken);

            var response = new PaginatedAiChatSessionsAPIModel_Response
            {
                Sessions = result.Sessions.Select(s => new AiChatSessionAPIModel_Response
                {
                    SessionId = s.SessionId,
                    EntityType = s.EntityType,
                    EntityKey = s.EntityKey,
                    Title = s.Title,
                    MessageCount = s.MessageCount,
                    CreatedAt = s.CreatedAt,
                    LastMessageAt = s.LastMessageAt
                }).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };

            return Ok(response);
        }

        /// <summary>
        /// Get a specific session with its full message history.
        /// </summary>
        /// <summary>
        /// Get a specific session with its full message history.
        /// </summary>
        [HttpGet("sessions/{sessionId:guid}")]
        [ProducesResponseType(typeof(AiChatSessionDetailAPIModel_Response), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetSessionAsync(
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            // === TEMP DEBUG: GetSessionAsync ===
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AiChatController>>();
            //logger.LogWarning("=== GetSessionAsync TOKEN CLAIMS ===");
            //logger.LogWarning("IsAuthenticated: {IsAuth}", HttpContext.User.Identity?.IsAuthenticated);
            //logger.LogWarning("AuthType: {AuthType}", HttpContext.User.Identity?.AuthenticationType);

            //foreach (var claim in HttpContext.User.Claims)
            //{
            //    logger.LogWarning("  Claim: {Type} = {Value}", claim.Type, claim.Value);
            //}

            //var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            //logger.LogWarning("Authorization header present: {HasAuth}, Value prefix: {Prefix}",
            //    !string.IsNullOrEmpty(authHeader),
            //    authHeader?.Length > 15 ? authHeader[..15] + "..." : authHeader ?? "(null)");
            // === END TEMP DEBUG ===

            var userId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

          //  logger.LogWarning("ClaimTypes.Name resolved to: '{UserId}'", userId ?? "(null)");

            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.LogWarning(">>> GetSessionAsync returning Unauthorized because ClaimTypes.Name is null/empty <<<");
                return Unauthorized();
            }

            var session = await _aiChatService.GetSessionWithMessagesAsync(
                userId, sessionId, cancellationToken);

            if (session == null)
            {
                logger.LogWarning(">>> GetSessionAsync returning NotFound for sessionId: {SessionId} <<<", sessionId);
                return NotFound($"Chat session not found: {sessionId}");
            }

            var response = new AiChatSessionDetailAPIModel_Response
            {
                SessionId = session.SessionId,
                EntityType = session.EntityType,
                EntityKey = session.EntityKey,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                LastMessageAt = session.LastMessageAt,
                Messages = session.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new AiChatMessageItemAPIModel
                    {
                        MessageId = m.MessageId,
                        Role = m.Role,
                        Content = m.Content,
                        TokensUsed = m.TokensUsed,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Soft-delete a chat session. The session is marked inactive
        /// and excluded from future listings.
        /// </summary>
        [HttpDelete("sessions/{sessionId:guid}")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> DeleteSessionAsync(
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var deleted = await _aiChatService.DeleteSessionAsync(
                userId, sessionId, cancellationToken);

            if (!deleted)
                return NotFound($"Chat session not found: {sessionId}");

            return NoContent();
        }
    }

    /// <summary>
    /// Paginated wrapper for chat session listings.
    /// </summary>
    public class PaginatedAiChatSessionsAPIModel_Response
    {
        public IList<AiChatSessionAPIModel_Response> Sessions { get; set; }
            = new List<AiChatSessionAPIModel_Response>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
