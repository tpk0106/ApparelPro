// ═══════════════════════════════════════════════════════════════════════════
//  AnomalyAlertController.cs — REST API Controller for Anomaly Detection
//  Location: ApparelPro.WebApi/Controllers/AI/AnomalyAlertController.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS THIS CONTROLLER?
// The HTTP entry point for the Phase 3 Anomaly Detection system.
// Unlike SopController (full CRUD), this is a READ + STATUS-UPDATE controller
// because anomaly alerts are system-generated, not user-created.
//
// 🎓 HOW IT FITS IN THE ARCHITECTURE:
//
//   React Frontend (Bell Icon + Notification Panel)
//       │
//       │  GET  /api/anomaly/alerts               → paginated list (notification panel)
//       │  GET  /api/anomaly/alerts/unread-count   → badge number for bell icon
//       │  GET  /api/anomaly/alerts/style          → alerts for a specific style
//       │  PUT  /api/anomaly/alerts/{id}/status    → acknowledge/resolve/dismiss
//       │  POST /api/anomaly/scan                  → trigger full scan (admin)
//       │  POST /api/anomaly/scan/style            → trigger style-specific scan
//       │
//       ▼
//   ┌─────────────────────────────────────────┐
//   │  AnomalyAlertController (this file)     │  ← HTTP layer
//   │  Maps AnomalyAlert → API models,        │     Lives in: ApparelPro.WebApi
//   │  extracts JWT claims, returns JSON      │
//   └──────────────┬──────────────────────────┘
//                  │
//                  ▼
//   ┌─────────────────────────────────────────┐
//   │  AnomalyDetectionService                │  ← AI/Detection engine
//   │  (IAnomalyDetectionService)             │     Lives in: ApparelPro.AI
//   │  Scans data, creates alerts, queries    │
//   └──────────────┬──────────────────────────┘
//                  │
//                  ▼
//   ┌─────────────────────────────────────────┐
//   │  ApparelProDbContext                    │  ← Data layer
//   │  AnomalyAlerts + AnomalyRules tables   │
//   └─────────────────────────────────────────┘
//
// 🎓 ROUTE: /api/anomaly
// Separate from /api/ai and /api/rag because anomaly detection is a distinct
// subsystem with its own lifecycle and data model.
//
// 🎓 WHY NO SERVICE LAYER IN BETWEEN?
// The AnomalyDetectionService in ApparelPro.AI already encapsulates all
// business logic (detection, deduplication, severity classification).
// Adding a BusinessLogic service in between would be an empty pass-through.
// This follows the same pattern as the AI chat controllers.
//
// 🎓 AUTHORIZATION:
// Uses JwtBearerDefaults + POLICY-BASED authorization (Stage 2 dynamic system).
// Two permission keys registered in PermissionService.DefaultCatalog:
//   "anomaly-alert-view"   → GET endpoints + style scan (AnomalyAlertViewRoles)
//   "anomaly-alert-manage" → full scan + status updates (AnomalyAlertManageRoles)
// Admins can adjust which roles have access via the Permission Matrix UI
// without requiring a code change — just re-seed after editing.
// ═══════════════════════════════════════════════════════════════════════════

using ApparelPro.AI.Abstractions;
using ApparelPro.WebApi.APIModels.AI;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApparelPro.WebApi.Controllers.AI
{
    /// <summary>
    /// 🎓 Anomaly Detection controller — Phase 3 AI Roadmap.
    ///
    /// Endpoints:
    ///   • GET  alerts           → paginated list with type/severity filters
    ///   • GET  alerts/unread-count → badge count for the bell icon
    ///   • GET  alerts/style     → alerts for a specific style
    ///   • PUT  alerts/{id}/status → update alert status
    ///   • POST scan             → trigger full scan (admin only)
    ///   • POST scan/style       → trigger scan for one style
    /// </summary>
    [Route("api/anomaly")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AnomalyAlertController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAnomalyDetectionService _anomalyService;

        /// <summary>
        /// 🎓 CONSTRUCTOR:
        ///   • IMapper — maps AnomalyAlert entity → AnomalyAlertAPIModel
        ///   • IAnomalyDetectionService — the detection engine abstraction (DIP)
        ///
        /// 🎓 WHY NO ApparelProDbContext?
        /// Controllers NEVER touch DbContext. The anomaly service owns all
        /// database operations internally via IServiceScopeFactory.
        /// </summary>
        public AnomalyAlertController(
            IMapper mapper,
            IAnomalyDetectionService anomalyService)
        {
            _mapper = mapper;
            _anomalyService = anomalyService;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  QUERY ENDPOINTS — Read alerts (available to all Merchandiser roles)
        // ═══════════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/anomaly/alerts — Paginated alert list for the notification panel
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 PAGINATED ALERT LIST:
        /// Powers the notification panel that opens when the user clicks the bell icon.
        /// Supports filtering by anomaly type, severity, AND status via query parameters.
        ///
        /// 🎓 QUERY PARAMETERS:
        ///   pageNumber        — 1-based page index (default: 1)
        ///   pageSize          — items per page (default: 10)
        ///   anomalyTypeFilter — "OVER_CONSUMPTION", "PRICE_SPIKE", or "WASTE_DAMAGE"
        ///   severityFilter    — "CRITICAL", "HIGH", "MEDIUM", or "LOW"
        ///   statusFilter      — "NEW", "READ", "ACKNOWLEDGED", "RESOLVED", or "DISMISSED"
        ///
        /// 🎓 STATUS FILTER BEHAVIOUR:
        /// When statusFilter is null (default), the service layer excludes RESOLVED
        /// and DISMISSED alerts — showing only actionable ones. When statusFilter is
        /// provided, it returns ONLY alerts matching that exact status, regardless
        /// of whether they're resolved or dismissed. This lets merchandisers review
        /// historical alerts by filtering to "RESOLVED" or "DISMISSED".
        ///
        /// 🎓 WHY NOT USE PaginationAPIModel?
        /// AnomalyDetectionService returns a simple tuple (Alerts, TotalCount)
        /// instead of PaginationResult. This is intentional — the anomaly
        /// service doesn't live in BusinessLogic and doesn't use the same
        /// pagination infrastructure. We build the response object manually.
        /// </summary>
        [HttpGet("alerts")]
        [Authorize(Policy = "anomaly-alert-view")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? anomalyTypeFilter = null,
            [FromQuery] string? severityFilter = null,
            [FromQuery] string? statusFilter = null)
        {
            var (alerts, totalCount) = await _anomalyService.GetActiveAlertsAsync(
                pageNumber, pageSize, anomalyTypeFilter, severityFilter, statusFilter);

            // 🎓 Map entity list → summary API model list (compact for the dropdown)
            var items = _mapper.Map<List<AnomalyAlertSummaryAPIModel>>(alerts);

            // 🎓 Build a pagination-like response manually since we're not using
            //    the BusinessLogic PaginationResult infrastructure
            return Ok(new
            {
                Items = items,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/anomaly/alerts/unread-count — Badge count for the bell icon
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 UNREAD COUNT:
        /// Returns an object { count: N } — the number of NEW alerts.
        /// The React frontend polls this endpoint every 30 seconds (via
        /// useUnreadAlertCount hook) to update the badge number on the bell icon.
        ///
        /// 🎓 WHY AN OBJECT AND NOT A BARE INTEGER?
        /// The frontend's TypeScript interface UnreadAlertCount expects { count: number }.
        /// Wrapping in an object is also better REST practice — it's extensible
        /// (we could add lastCheckedAt later) and self-documenting in the JSON response.
        ///
        /// 🎓 WHY A SEPARATE ENDPOINT?
        /// This is much cheaper than loading the full alert list just to count them.
        /// SELECT COUNT(*) WHERE Status = 'NEW' is a single indexed scan.
        /// </summary>
        [HttpGet("alerts/unread-count")]
        [Authorize(Policy = "anomaly-alert-view")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCountAsync()
        {
            var count = await _anomalyService.GetUnreadCountAsync();

            // 🎓 Return as { count: N } to match the frontend's UnreadAlertCount interface.
            // Previously returned Ok(count) which sent a bare integer — caused
            // response.data.count to be undefined on the React side.
            return Ok(new { Count = count });
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/anomaly/alerts/recent — Latest alerts for bell dropdown
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 RECENT ALERTS:
        /// Returns the latest ~10 alerts as compact summaries for the bell dropdown.
        /// Unlike the paginated /alerts endpoint, this is a simple "give me the latest"
        /// query with no filters, no pagination metadata, and no total count.
        ///
        /// 🎓 FRONTEND CONSUMER:
        /// The useRecentAlerts hook calls this only when the bell dropdown is opened
        /// (enabled: dropdownOpen), so it doesn't fire on every page load.
        ///
        /// 🎓 ROUTE ORDER NOTE:
        /// This must be defined BEFORE /alerts/style because ASP.NET routes are
        /// matched in order, and "recent" could theoretically conflict with the
        /// {alertId} parameter in alerts/{alertId}/status. By using a distinct
        /// sub-path "alerts/recent", there's no ambiguity.
        /// </summary>
        [HttpGet("alerts/recent")]
        [Authorize(Policy = "anomaly-alert-view")]
        [ProducesResponseType(typeof(List<AnomalyAlertSummaryAPIModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentAlertsAsync([FromQuery] int limit = 10)
        {
            var alerts = await _anomalyService.GetRecentAlertsAsync(limit);

            // 🎓 Map to the SUMMARY model (compact) — same model used in the
            //    paginated list. The bell dropdown only needs type, severity,
            //    description, style info, and timestamp — not the full detail.
            var summaries = _mapper.Map<List<AnomalyAlertSummaryAPIModel>>(alerts);
            return Ok(summaries);
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/anomaly/alerts/style — Alerts for a specific style
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 STYLE-SPECIFIC ALERTS:
        /// Returns all active (unresolved) alerts for a given style.
        /// Used in two places:
        ///   1. Style detail page — shows a warning banner if anomalies exist
        ///   2. AI chat context — injected into the prompt so Claude can mention them
        ///
        /// 🎓 QUERY PARAMETERS:
        /// Uses the standard 4-part composite key (BuyerCode/Order/TypeCode/StyleCode)
        /// that identifies a style throughout the system.
        /// </summary>
        [HttpGet("alerts/style")]
        [Authorize(Policy = "anomaly-alert-view")]
        [ProducesResponseType(typeof(List<AnomalyAlertAPIModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertsForStyleAsync(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            var alerts = await _anomalyService.GetActiveAlertsForStyleAsync(
                buyerCode, order, typeCode, styleCode);

            // 🎓 Full model here (not summary) — the style detail page needs all fields
            var apiModels = _mapper.Map<List<AnomalyAlertAPIModel>>(alerts);
            return Ok(apiModels);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  STATUS UPDATE — Acknowledge, resolve, or dismiss an alert
        // ═══════════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────────────────────
        //  PUT /api/anomaly/alerts/{id}/status — Update alert status
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 STATUS UPDATE:
        /// The ONLY write operation users perform on alerts.
        /// Updates the status and records who acted and when.
        ///
        /// 🎓 VALID STATUS VALUES:
        ///   "READ"          — user has viewed the alert
        ///   "ACKNOWLEDGED"  — user confirms they've seen it
        ///   "RESOLVED"      — the underlying issue has been fixed
        ///   "DISMISSED"     — user decided it's not a real issue
        ///
        /// 🎓 JWT CLAIM EXTRACTION:
        /// UserId comes from the JWT token — NOT from the request body.
        /// This follows the security protocol: "securely extract the caller's
        /// contextual parameters directly from the token identity profile."
        /// </summary>
        [HttpPut("alerts/{alertId}/status")]
        [Authorize(Policy = "anomaly-alert-manage")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAlertStatusAsync(
            Guid alertId,
            [FromBody] UpdateAlertStatusAPIModel model)
        {
            // 🎓 Validate the new status is one of the allowed values
            var validStatuses = new[] { "READ", "ACKNOWLEDGED", "RESOLVED", "DISMISSED" };
            if (!validStatuses.Contains(model.NewStatus.ToUpperInvariant()))
            {
                return BadRequest(new
                {
                    Error = $"Invalid status '{model.NewStatus}'. " +
                            $"Valid values: {string.Join(", ", validStatuses)}"
                });
            }

            // 🎓 Extract UserId from JWT — never trust the client to send it
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(ClaimTypes.Name)?.Value
                         ?? "unknown";

            var success = await _anomalyService.UpdateAlertStatusAsync(
                alertId, model.NewStatus.ToUpperInvariant(), userId, model.UserNote);

            if (!success)
                return NotFound(new { Error = $"Alert {alertId} not found." });

            return NoContent();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  SCAN TRIGGERS — Manually trigger anomaly detection
        // ═══════════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────────────────────
        //  POST /api/anomaly/scan — Trigger a full scan (admin only)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 FULL SCAN TRIGGER:
        /// Runs anomaly detection across ALL active styles.
        /// This is an admin action — normally the background job does this automatically.
        /// Useful for:
        ///   • After a bulk data import
        ///   • After changing anomaly rule thresholds
        ///   • For immediate visibility before the next scheduled scan
        ///
        /// 🎓 RESTRICTED TO MANAGERS:
        /// Only Merchandiser Manager and Administrator can trigger full scans
        /// because they're computationally expensive (scans every style).
        /// </summary>
        [HttpPost("scan")]
        [Authorize(Policy = "anomaly-alert-manage")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> TriggerFullScanAsync()
        {
            var newAlertCount = await _anomalyService.RunFullScanAsync();

            return Ok(new
            {
                Message = "Full anomaly scan completed.",
                NewAlertsCreated = newAlertCount
            });
        }

        // ─────────────────────────────────────────────────────────────────
        //  POST /api/anomaly/scan/style — Trigger a style-specific scan
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 TARGETED SCAN:
        /// Runs anomaly detection for a SINGLE style only.
        /// Called when a user navigates to a style detail page — provides
        /// fresh anomaly data without waiting for the next background scan.
        ///
        /// 🎓 WHY POST AND NOT GET?
        /// This endpoint has SIDE EFFECTS — it creates new AnomalyAlert records
        /// in the database. GET should be idempotent; POST signals mutation.
        /// </summary>
        [HttpPost("scan/style")]
        [Authorize(Policy = "anomaly-alert-view")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> TriggerStyleScanAsync(
            [FromBody] ScanStyleRequestAPIModel model)
        {
            var newAlertCount = await _anomalyService.ScanStyleAsync(
                model.BuyerCode, model.Order, model.TypeCode, model.StyleCode);

            return Ok(new
            {
                Message = $"Style scan completed for {model.BuyerCode}/{model.Order}/{model.TypeCode}/{model.StyleCode}.",
                NewAlertsCreated = newAlertCount
            });
        }
    }
}
