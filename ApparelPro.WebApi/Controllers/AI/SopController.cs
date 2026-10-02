// ═══════════════════════════════════════════════════════════════════════════
//  SopController.cs — REST API Controller for SOP Admin CRUD
//  Location: ApparelPro.WebApi/Controllers/AI/SopController.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS THIS CONTROLLER?
// This is the HTTP entry point for managing Standard Operating Procedures.
// It provides full CRUD (Create, Read, Update, Delete) endpoints for the
// SOP Admin UI, following the same patterns as StyleDetailsController.
//
// 🎓 HOW IT FITS IN THE ARCHITECTURE:
//
//   React Frontend (SopAdminPage)
//       │
//       │  GET  /api/sop/list          → paginated list
//       │  GET  /api/sop/{id}          → single SOP with applicability rules
//       │  POST /api/sop               → create SOP + rules
//       │  PUT  /api/sop               → update SOP + replace rules
//       │  DELETE /api/sop/{id}        → soft delete (sets IsActive = false)
//       │
//       ▼
//   ┌─────────────────────────────────────────┐
//   │  SopController (this file)              │  ← HTTP layer
//   │  Validates input, maps API↔Service,     │     Lives in: ApparelPro.WebApi
//   │  extracts JWT claims, returns JSON      │
//   └──────────────┬──────────────────────────┘
//                  │
//                  ▼
//   ┌─────────────────────────────────────────┐
//   │  SopService (ISopService)               │  ← Business logic layer
//   │  CRUD + GetActiveSopsForContextAsync    │     Lives in: apparelPro.BusinessLogic
//   └──────────────┬──────────────────────────┘
//                  │
//                  ▼
//   ┌─────────────────────────────────────────┐
//   │  ApparelProDbContext                    │  ← Data layer
//   │  EF Core → SQL Server                  │
//   └─────────────────────────────────────────┘
//
// 🎓 ROUTE: /api/sop
// Separate from /api/ai and /api/rag because SOPs are a distinct domain:
//   /api/ai/summarise → "Summarise THIS entity" (AI operations)
//   /api/rag/query    → "Search ALL entities" (RAG discovery)
//   /api/sop          → "Manage SOP records" (CRUD admin)
//
// 🎓 AUTHORIZATION:
// Uses the "sop-management" policy — only Merchandiser Manager and Administrator
// roles can manage SOPs. This is registered in DefaultCatalog.cs (PermissionService).
//
// 🎓 JWT CLAIM EXTRACTION:
// CreatedBy/ModifiedBy are NOT sent by the client — they're extracted from the
// JWT token using User.FindFirst(ClaimTypes.Name).Value. This follows the
// security protocol: "do NOT create input fields or pass them as parameters.
// Instead, securely extract the caller's contextual parameters directly from
// the token identity profile."
// ═══════════════════════════════════════════════════════════════════════════

using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.AI;
using apparelPro.BusinessLogic.Services.Models.AI.ISopService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.AI;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApparelPro.WebApi.Controllers.AI
{
    /// <summary>
    /// 🎓 CRUD controller for Standard Operating Procedures.
    ///
    /// Follows the StyleDetailsController pattern:
    ///   • Constructor: IMapper + ISopService (DIP — depends on abstraction)
    ///   • List: Paginated with PaginationAPIModel wrapper
    ///   • GetById: Returns full SOP + nested applicability rules
    ///   • Create: Extracts CreatedBy from JWT, returns CreatedAtRoute
    ///   • Update: Extracts ModifiedBy from JWT, returns NoContent
    ///   • Delete: Soft delete via service layer, returns NoContent
    /// </summary>
    [Route("api/sop")]
    [ApiController]
    [Authorize(Policy = "sop-management")]
    public class SopController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISopService _sopService;

        /// <summary>
        /// 🎓 CONSTRUCTOR:
        /// Follows the mandatory constructor pattern from clipper-migration skill:
        ///   • IMapper for API ↔ Service model conversions
        ///   • ISopService abstraction (not the concrete SopService)
        ///
        /// 🎓 WHY NO ApparelProDbContext?
        /// Controllers NEVER touch the DbContext directly. The service layer
        /// owns all database operations. This is the Dependency Inversion
        /// Principle (DIP) in action.
        /// </summary>
        public SopController(
            IMapper mapper,
            ISopService sopService)
        {
            _mapper = mapper;
            _sopService = sopService;
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/sop/list — Paginated list of SOPs
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 PAGINATED LIST ENDPOINT:
        /// Returns a paged, sortable, filterable list of SOPs.
        ///
        /// 🎓 QUERY PARAMETERS (all optional except pageSize/pageNumber):
        ///   pageSize     — items per page (e.g., 10, 25, 50)
        ///   pageNumber   — which page to return (1-based)
        ///   sortColumn   — column name to sort by (e.g., "SopCode", "Title")
        ///   sortOrder    — "asc" or "desc"
        ///   filterColumn — column name to filter on
        ///   filterQuery  — filter value (partial match)
        ///
        /// 🎓 RESPONSE STRUCTURE (PaginationAPIModel):
        ///   {
        ///     "items": [ { sopId, sopCode, title, ... }, ... ],
        ///     "pageSize": 10,
        ///     "currentPage": 1,
        ///     "totalItems": 42,
        ///     "totalPages": 5,
        ///     "sortColumn": "SopCode",
        ///     "sortOrder": "asc"
        ///   }
        ///
        /// 🎓 WHY PaginationAPIModel?
        /// The AutoMapper converter PaginationResultToPaginationAPITypeConverter
        /// handles the PaginationResult → PaginationAPIModel mapping automatically.
        /// This is registered in ServicetoAPIModelMappings.cs.
        /// </summary>
        [HttpGet("list")]
        [ProducesResponseType(typeof(PaginationAPIModel<SopAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSopsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            // 🎓 Step 1: Call the service layer with pagination parameters.
            // The service returns PaginationResult<SopServiceModel>.
            var sopServiceModels = await _sopService.GetSopsAsync(
                pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);

            // 🎓 Step 2: Map PaginationResult<SopServiceModel> → PaginationAPIModel<SopAPIModel>.
            // The PaginationResultToPaginationAPITypeConverter handles the outer wrapper,
            // and the inner CreateMap<SopServiceModel, SopAPIModel> handles each item.
            var sopsPage = _mapper.Map<PaginationAPIModel<SopAPIModel>>(sopServiceModels);

            return Ok(sopsPage);
        }

        // ─────────────────────────────────────────────────────────────────
        //  GET /api/sop/{id} — Single SOP by ID
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 GET BY ID:
        /// Returns a single SOP with its nested applicability rules.
        ///
        /// 🎓 WHY INCLUDE APPLICABILITY RULES?
        /// The edit dialog needs the full SOP + rules to pre-populate the form.
        /// The service layer uses .Include(s => s.SopApplicabilities) to eager-load
        /// the child collection.
        ///
        /// 🎓 ERROR HANDLING:
        /// If the SOP doesn't exist, returns 422 Unprocessable Entity (not 404).
        /// This matches the project convention in StyleDetailsController.
        /// </summary>
        [HttpGet("{id}", Name = "GetSopByIdAsync")]
        [ProducesResponseType(typeof(SopAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetSopByIdAsync([FromRoute] int id)
        {
            // 🎓 The service layer returns null if not found.
            var sopServiceModel = await _sopService.GetSopByIdAsync(id);

            if (sopServiceModel == null)
            {
                return UnprocessableEntity($"SOP with ID {id} was not found.");
            }

            var sopAPIModel = _mapper.Map<SopAPIModel>(sopServiceModel);
            return Ok(sopAPIModel);
        }

        // ─────────────────────────────────────────────────────────────────
        //  POST /api/sop — Create a new SOP
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 CREATE ENDPOINT:
        /// Creates a new SOP + its applicability rules in a single transaction.
        ///
        /// 🎓 JWT CLAIM EXTRACTION:
        /// CreatedBy is extracted from the JWT token (ClaimTypes.Name = user email).
        /// The frontend NEVER sends CreatedBy — it's set server-side for security.
        ///
        /// 🎓 RESPONSE:
        /// Returns 201 Created with a Location header pointing to the new SOP:
        ///   Location: /api/sop/{newSopId}
        ///
        /// 🎓 ERROR HANDLING:
        /// InvalidOperationException from the service layer (e.g., duplicate SopCode)
        /// is caught and returned as 400 Bad Request with the error message.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddSopAsync(
            [FromBody] CreateSopAPIModel createSopAPIModel)
        {
            try
            {
                // 🎓 Step 1: Extract the current user's email from the JWT token.
                // This follows the security protocol — never trust client-supplied
                // identity fields.
                var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;

                // 🎓 Step 2: Map API model → Service model.
                var createSopServiceModel = _mapper.Map<CreateSopServiceModel>(createSopAPIModel);

                // 🎓 Step 3: Set CreatedBy from the JWT claim.
                // The API model doesn't have this field — we inject it after mapping.
                createSopServiceModel.CreatedBy = currentUserEmail ?? "unknown";

                // 🎓 Step 4: Call the service to create the SOP + rules.
                // The service wraps both inserts in a transaction.
                var addedSop = await _sopService.AddSopAsync(createSopServiceModel);

                // 🎓 Step 5: Return 201 Created with Location header.
                // CreatedAtRoute generates: Location: /api/sop/{addedSop.SopId}
                return CreatedAtRoute(
                    nameof(GetSopByIdAsync),
                    new { id = addedSop.SopId },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                // 🎓 Business rule violation (e.g., duplicate SopCode).
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  PUT /api/sop — Update an existing SOP
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 UPDATE ENDPOINT:
        /// Updates an existing SOP and REPLACES all its applicability rules.
        ///
        /// 🎓 REPLACE-CHILDREN PATTERN:
        /// The service layer deletes ALL existing applicability rules for this SOP
        /// and re-creates them from the request body. This is simpler than tracking
        /// individual adds/removes/updates and works well for the small number of
        /// rules per SOP (typically 1-5).
        ///
        /// 🎓 JWT CLAIM EXTRACTION:
        /// ModifiedBy is extracted from the JWT token, same as CreatedBy in create.
        ///
        /// 🎓 VALIDATION:
        /// The controller checks that the SOP exists before updating. If not found,
        /// returns 422 Unprocessable Entity.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> UpdateSopAsync(
            [FromBody] UpdateSopAPIModel updateSopAPIModel)
        {
            // 🎓 Step 1: Verify the SOP exists.
            var existingSop = await _sopService.GetSopByIdAsync(updateSopAPIModel.SopId);
            if (existingSop == null)
            {
                return UnprocessableEntity($"SOP with ID {updateSopAPIModel.SopId} was not found.");
            }

            try
            {
                // 🎓 Step 2: Extract the current user's email from the JWT token.
                var currentUserEmail = User.FindFirst(ClaimTypes.Name)?.Value;

                // 🎓 Step 3: Map API model → Service model.
                var updateSopServiceModel = _mapper.Map<UpdateSopServiceModel>(updateSopAPIModel);

                // 🎓 Step 4: Set ModifiedBy from the JWT claim.
                updateSopServiceModel.ModifiedBy = currentUserEmail ?? "unknown";

                // 🎓 Step 5: Call the service to update SOP + replace rules.
                await _sopService.UpdateSopAsync(updateSopServiceModel);
            }
            catch (InvalidOperationException ex)
            {
                // 🎓 Business rule violation (e.g., SopCode conflict).
                return BadRequest(new { message = ex.Message });
            }

            // 🎓 Step 6: Return 204 No Content (standard for successful updates).
            return NoContent();
        }

        // ─────────────────────────────────────────────────────────────────
        //  DELETE /api/sop/{id} — Soft delete an SOP
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 🎓 DELETE ENDPOINT (SOFT DELETE):
        /// Sets IsActive = false on the SOP rather than physically deleting it.
        ///
        /// 🎓 WHY SOFT DELETE?
        /// SOPs may have been referenced in previously generated PDFs. Hard deleting
        /// would break the audit trail. Soft delete preserves the data while hiding
        /// the SOP from active queries (GetActiveSopsForContextAsync filters by IsActive).
        ///
        /// 🎓 THE SERVICE LAYER HANDLES THE SOFT DELETE:
        /// SopService.DeleteSopAsync(sopId) sets IsActive = false and saves.
        /// The controller just validates existence and calls the service.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteSopAsync([FromRoute] int id)
        {
            // 🎓 Step 1: Verify the SOP exists.
            var existingSop = await _sopService.GetSopByIdAsync(id);
            if (existingSop == null)
            {
                return UnprocessableEntity($"SOP with ID {id} was not found.");
            }

            try
            {
                // 🎓 Step 2: Call the service to soft delete.
                await _sopService.DeleteSopAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            // 🎓 Step 3: Return 204 No Content.
            return NoContent();
        }
    }
}
