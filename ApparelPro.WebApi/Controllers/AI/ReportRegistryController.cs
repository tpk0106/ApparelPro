// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistryController.cs — API Controller
//  Location: ApparelPro.WebApi/Controllers/AI/ReportRegistryController.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 CONTROLLER CONVENTION:
// Controllers live under:
//   ApparelPro.WebApi/Controllers/ (flat, or domain-grouped subfolder)
//
// We place this under Controllers/AI/ to keep AI-related controllers together
// with the existing RagController.cs.
//
// 🎓 ROUTE CONVENTION:
// Routes use kebab-case: "api/report-registry"
// This matches "api/trim-sheet-report", "api/styleDetails", etc.
//
// 🎓 AUTHORIZATION:
// Uses policy-based authorization matching the project's pattern.
// The "style-details" policy is reused here since anyone who can view
// style details should also be able to see available reports.
// A dedicated "report-registry" policy can be created later if needed.
// ═══════════════════════════════════════════════════════════════════════════

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.AI;

namespace ApparelPro.WebApi.Controllers.AI
{
    /// <summary>
    /// 🎓 REST API for the report registry catalogue.
    ///
    /// Currently read-only — provides listing endpoints for:
    ///   1. Frontend admin screens (list all available reports)
    ///   2. Diagnostics (verify which reports the RAG pipeline can detect)
    ///
    /// 🎓 NOTE: The RAG pipeline itself does NOT call this controller.
    /// RagService will call IReportRegistryService directly via DI.
    /// This controller exists for external consumers (frontend, monitoring).
    /// </summary>
    [Route("api/report-registry")]
    [ApiController]
    public class ReportRegistryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IReportRegistryService _reportRegistryService;

        public ReportRegistryController(IMapper mapper, IReportRegistryService reportRegistryService)
        {
            _mapper = mapper;
            _reportRegistryService = reportRegistryService;
        }

        // ── GET: api/report-registry/active ──────────────────────────────
        /// <summary>
        /// 🎓 Returns all active reports in the registry, ordered by DisplayOrder.
        ///
        /// The frontend can use this to:
        ///   • Show a "Available Reports" section in the RAG panel
        ///   • Build a report picker dropdown
        ///   • Display report descriptions as help text
        ///
        /// Returns: IEnumerable&lt;ReportRegistryAPIModel&gt;
        /// </summary>
        [HttpGet("active")]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(typeof(IEnumerable<ReportRegistryAPIModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveReportsAsync()
        {
            var serviceModels = await _reportRegistryService.GetActiveReportsAsync();
            var apiModels = _mapper.Map<IEnumerable<ReportRegistryAPIModel>>(serviceModels);
            return Ok(apiModels);
        }

        // ── GET: api/report-registry/{reportCode} ───────────────────────
        /// <summary>
        /// 🎓 Returns a single report by its code.
        ///
        /// Useful for:
        ///   • Verifying a specific report exists and is active
        ///   • Getting details for a report the user selected
        ///
        /// Returns: ReportRegistryAPIModel or 404 if not found/inactive
        /// </summary>
        [HttpGet("{reportCode}")]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(typeof(ReportRegistryAPIModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportByCodeAsync(string reportCode)
        {
            var serviceModel = await _reportRegistryService.GetReportByCodeAsync(reportCode);

            if (serviceModel == null)
                return NotFound(new { Error = $"Report '{reportCode}' not found or is inactive." });

            var apiModel = _mapper.Map<ReportRegistryAPIModel>(serviceModel);
            return Ok(apiModel);
        }

        // ── GET: api/report-registry/category/{category} ────────────────
        /// <summary>
        /// 🎓 Returns all active reports in a specific category.
        ///
        /// Categories match the navigation structure:
        ///   "OrderManagement", "Inventory", "Shipments", etc.
        ///
        /// Returns: IEnumerable&lt;ReportRegistryAPIModel&gt; (may be empty)
        /// </summary>
        [HttpGet("category/{category}")]
        [Authorize(Policy = "style-details")]
        [ProducesResponseType(typeof(IEnumerable<ReportRegistryAPIModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportsByCategoryAsync(string category)
        {
            var serviceModels = await _reportRegistryService.GetReportsByCategoryAsync(category);
            var apiModels = _mapper.Map<IEnumerable<ReportRegistryAPIModel>>(serviceModels);
            return Ok(apiModels);
        }
    }
}
