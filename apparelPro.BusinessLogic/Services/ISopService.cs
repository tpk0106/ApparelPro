// ═══════════════════════════════════════════════════════════════════════════
//  ISopService.cs — Service Interface
//  Location: apparelPro.BusinessLogic/Services/ISopService.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 INTERFACE CONVENTION:
// Service interfaces live flat in:
//   apparelPro.BusinessLogic/Services/I{Entity}Service.cs
//
// NOT grouped by domain folder — matches IBuyerService.cs, IReportRegistryService.cs.
//
// 🎓 FULL CRUD + PAGINATION:
// Unlike IReportRegistryService (read-only), ISopService has full CRUD
// because SOPs are user-managed records with an admin UI.
// Pagination follows the exact same pattern as IBuyerService.
//
// 🎓 SPECIAL METHODS:
// - GetActiveSopsForContextAsync: The query the PDF engine calls to find
//   which SOPs should be injected into a report. Takes contextual parameters
//   (report type, buyer code, supplier code) and returns matching SOPs
//   after evaluating applicability rules and exclusions.
// ═══════════════════════════════════════════════════════════════════════════

using apparelPro.BusinessLogic.Services.Models.AI.ISopService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    /// <summary>
    /// 🎓 Contract for managing Standard Operating Procedures.
    ///
    /// PRIMARY CONSUMERS:
    ///   1. SopController — exposes full CRUD via REST API for admin UI
    ///   2. TrimSheetReportEngine (Phase 2 Step 5) — calls GetActiveSopsForContextAsync
    ///      to inject matching SOPs into PDF reports
    ///   3. EntityChunkerService (Phase 2 Step 6) — reads active SOPs for RAG embedding
    /// </summary>
    public interface ISopService
    {
        /// <summary>
        /// 🎓 Paginated list of all SOPs for the admin grid.
        /// Supports sorting, filtering — same contract as IBuyerService.GetBuyersAsync.
        /// </summary>
        Task<PaginationResult<SopServiceModel>> GetSopsAsync(
            int pageNumber, int pageSize,
            string? sortColumn, string? sortOrder,
            string? filterColumn, string? filterQuery);

        /// <summary>
        /// 🎓 Returns a single SOP by its ID, including nested applicability rules.
        /// Used by the admin UI when opening an SOP for editing.
        /// Returns null if not found.
        /// </summary>
        Task<SopServiceModel?> GetSopByIdAsync(int sopId);

        /// <summary>
        /// 🎓 Returns a single SOP by its unique business code.
        /// Useful for lookups by code rather than PK (e.g., from RAG context).
        /// Returns null if not found.
        /// </summary>
        Task<SopServiceModel?> GetSopByCodeAsync(string sopCode);

        /// <summary>
        /// 🎓 Creates a new SOP with its applicability rules in a single transaction.
        /// Returns the created SOP with server-generated SopId and audit fields.
        /// </summary>
        Task<SopServiceModel> AddSopAsync(CreateSopServiceModel createSopServiceModel);

        /// <summary>
        /// 🎓 Updates an existing SOP and REPLACES its applicability rules.
        /// The service deletes existing rules and re-creates from the model's list.
        /// This "replace all children" approach is simpler than individual add/remove
        /// and perfectly fine for the small number of rules per SOP (typically 1-5).
        /// </summary>
        Task UpdateSopAsync(UpdateSopServiceModel updateSopServiceModel);

        /// <summary>
        /// 🎓 Soft-delete: sets IsActive = false rather than physically removing the record.
        /// SOPs might be referenced in historical PDFs, so we never hard-delete.
        /// </summary>
        Task DeleteSopAsync(int sopId);

        /// <summary>
        /// 🎓 THE KEY QUERY FOR PDF INJECTION (Phase 2 Step 5).
        ///
        /// Given a report context (reportCode, buyerCode, supplierCode),
        /// returns all SOPs that should appear on that report, after evaluating:
        ///   1. Applicability rules (inclusions via ApplicabilityType + ApplicabilityKey)
        ///   2. Exclusion overrides (IsExcluded = true rules)
        ///   3. Date validity (EffectiveFrom ≤ today ≤ EffectiveTo)
        ///   4. Active status (IsActive = true)
        ///
        /// Parameters are nullable because not every report context has all three.
        /// For Trim Sheet: reportCode = "TrimSheet", buyerCode = "101", supplierCode = null.
        /// </summary>
        Task<IEnumerable<SopServiceModel>> GetActiveSopsForContextAsync(
            string? reportCode, string? buyerCode, string? supplierCode);
    }
}
