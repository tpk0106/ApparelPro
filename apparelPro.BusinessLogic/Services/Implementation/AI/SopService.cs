// ═══════════════════════════════════════════════════════════════════════════
//  SopService.cs — Service Implementation
//  Location: apparelPro.BusinessLogic/Services/Implementation/AI/SopService.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 SERVICE IMPLEMENTATION CONVENTION:
// Implementations live under:
//   apparelPro.BusinessLogic/Services/Implementation/{DomainFolder}/
//
// This matches ReportRegistryService.cs under Implementation/AI/.
//
// 🎓 CONSTRUCTOR PATTERN:
// Every service follows this exact constructor pattern:
//   1. Inject IMapper and ApparelProDbContext (mandatory for all services)
//   2. Explicit null checks using if-throw (not ?? throw)
//   3. Private readonly fields with underscore prefix
//
// 🎓 CRUD + PAGINATION:
// Unlike ReportRegistryService (read-only), SopService has full CRUD
// with the same pagination pattern as BuyerService.
//
// 🎓 KEY METHOD — GetActiveSopsForContextAsync:
// This is the query that the PDF engine (Phase 2 Step 5) calls to find
// which SOPs should appear on a report. It evaluates:
//   1. Applicability rules (inclusions)
//   2. Exclusion overrides
//   3. Date-bounded validity
//   4. Active status
// ═══════════════════════════════════════════════════════════════════════════

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ApparelPro.Data;
using ApparelPro.Data.Models.AI;
using ApparelPro.Shared.Extensions;
using apparelPro.BusinessLogic.Services.Models.AI.ISopService;
using apparelPro.BusinessLogic.Misc;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.AI
{
    /// <summary>
    /// 🎓 Concrete implementation of ISopService.
    ///
    /// Provides full CRUD for Standard Operating Procedures and the context-aware
    /// query used by the PDF engine to inject SOPs into reports.
    ///
    /// 🎓 LIFETIME: Registered as TRANSIENT in DI (same as ReportRegistryService).
    /// </summary>
    public class SopService : ISopService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        /// <summary>
        /// 🎓 CONSTRUCTOR INJECTION:
        /// Follows the exact same pattern as ReportRegistryService:
        ///   • IMapper for Entity ↔ ServiceModel conversions
        ///   • ApparelProDbContext for database access
        ///   • Explicit null checks (project convention)
        /// </summary>
        public SopService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            if (apparelProDbContext == null) throw new ArgumentNullException(nameof(apparelProDbContext));

            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        /// <inheritdoc />
        public async Task<PaginationResult<SopServiceModel>> GetSopsAsync(
            int pageNumber, int pageSize,
            string? sortColumn, string? sortOrder,
            string? filterColumn, string? filterQuery)
        {
            // 🎓 PAGINATION PATTERN:
            // Follows the exact same approach as BuyerService.GetBuyersAsync:
            //   1. Build base IQueryable with .AsNoTracking()
            //   2. Apply filter if provided (using InputValidator + Dynamic LINQ)
            //   3. Count total before paging
            //   4. Apply sort
            //   5. Apply Skip/Take for paging
            //   6. Execute query and map to service models

            var query = _apparelProDbContext.StandardOperatingProcedures
                .AsNoTracking()
                .Include(sop => sop.SopApplicabilities)
                .AsQueryable();

            // 🎓 Dynamic filtering — same pattern as BuyerService.
            // InputValidator sanitises the filter column name against the entity type
            // to prevent SQL injection via Dynamic LINQ.
            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(StandardOperatingProcedure));
                query = query.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = await query.CountAsync();

            // 🎓 Dynamic sorting — same pattern as BuyerService.
            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                query = query.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            else
            {
                // 🎓 Default sort by DisplayOrder when no sort column specified.
                query = query.OrderBy(sop => sop.DisplayOrder);
            }

            query = query
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var entities = await query.ToListAsync();
            var serviceModels = _mapper.Map<IList<SopServiceModel>>(entities);

            return new PaginationResult<SopServiceModel>(pageSize, pageNumber, counter, serviceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        /// <inheritdoc />
        public async Task<SopServiceModel?> GetSopByIdAsync(int sopId)
        {
            // 🎓 Include applicability rules so the admin UI can display them
            // alongside the SOP in the edit form.
            var entity = await _apparelProDbContext.StandardOperatingProcedures
                .AsNoTracking()
                .Include(sop => sop.SopApplicabilities)
                .FirstOrDefaultAsync(sop => sop.SopId == sopId);

            return entity == null ? null : _mapper.Map<SopServiceModel>(entity);
        }

        /// <inheritdoc />
        public async Task<SopServiceModel?> GetSopByCodeAsync(string sopCode)
        {
            var entity = await _apparelProDbContext.StandardOperatingProcedures
                .AsNoTracking()
                .Include(sop => sop.SopApplicabilities)
                .FirstOrDefaultAsync(sop => sop.SopCode == sopCode);

            return entity == null ? null : _mapper.Map<SopServiceModel>(entity);
        }

        /// <inheritdoc />
        public async Task<SopServiceModel> AddSopAsync(CreateSopServiceModel createSopServiceModel)
        {
            // 🎓 TRANSACTION PATTERN:
            // Wrap in a transaction because we're inserting both the parent SOP
            // and its child applicability rules. If the child inserts fail,
            // the parent insert rolls back too.
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = _mapper.Map<StandardOperatingProcedure>(createSopServiceModel);

                // 🎓 Server-set audit fields — NOT passed from the client.
                entity.CreatedAt = DateTime.UtcNow;
                entity.ModifiedBy = entity.CreatedBy;
                entity.ModifiedAt = entity.CreatedAt;

                // 🎓 Bind parent FK on each child applicability rule.
                // After EF Core saves the parent, SopId is populated via identity.
                // But we add children to the navigation property so EF Core
                // handles the FK assignment automatically.

                await _apparelProDbContext.StandardOperatingProcedures.AddAsync(entity);
                await _apparelProDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // 🎓 Re-read with Include to return the full model with children.
                return (await GetSopByIdAsync(entity.SopId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task UpdateSopAsync(UpdateSopServiceModel updateSopServiceModel)
        {
            // 🎓 REPLACE-CHILDREN PATTERN:
            // Instead of tracking individual add/remove/update of applicability rules,
            // we delete ALL existing rules and re-create from the model's list.
            // This is the simplest approach for small child collections (1-5 rules per SOP).

            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            try
            {
                var existing = await _apparelProDbContext.StandardOperatingProcedures
                    .Include(sop => sop.SopApplicabilities)
                    .FirstOrDefaultAsync(sop => sop.SopId == updateSopServiceModel.SopId);

                if (existing == null)
                {
                    throw new InvalidOperationException(
                        $"SOP with ID {updateSopServiceModel.SopId} not found.");
                }

                // ═══════════════════════════════════════════════════════════
                // 🎓 CRITICAL FIX — SAFE CHILD-REPLACEMENT WITH AUTOMAPPER
                //
                // PROBLEM:
                // AutoMapper's Map(source, destination) maps ALL properties,
                // INCLUDING the SopApplicabilities navigation collection.
                // This causes TWO bugs:
                //
                //   BUG 1 — "Temporary value" crash:
                //   AutoMapper REPLACES the tracked DB entities (real PKs)
                //   with brand-new entities (temp PK = 0). If RemoveRange
                //   is called on these ghosts, EF Core throws:
                //     "SopApplicability.SopApplicabilityId has a temporary
                //      value while attempting to change state to 'Deleted'."
                //
                //   BUG 2 — Ghost insertion (duplicate rows):
                //   The ghost entities sit in the tracked navigation property.
                //   When SaveChangesAsync runs, EF Core's DetectChanges walks
                //   the navigation and finds them as new Added entities. It
                //   INSERTs them alongside the intended deletes, creating
                //   duplicate applicability rules in the database.
                //
                // SOLUTION:
                //   1. Capture original DB children BEFORE AutoMapper
                //   2. Run AutoMapper (maps scalars + creates ghosts)
                //   3. IMMEDIATELY detach ghosts from change tracker
                //   4. Delete originals (Phase 1) — clean save, no ghosts
                //   5. Add new rules (Phase 2) — only the intended inserts
                // ═══════════════════════════════════════════════════════════

                // ── Step 1: Snapshot original DB entities ────────────────
                // 🎓 These have REAL PKs from the database and are properly
                // tracked by EF Core. We'll delete these in Phase 1.
                var originalRules = existing.SopApplicabilities.ToList();

                // ── Step 2: Map scalar properties via AutoMapper ─────────
                // 🎓 This maps Title, Description, IsActive, Category, etc.
                // SIDE EFFECT: Also overwrites SopApplicabilities with ghost
                // entities (temp PKs). We handle that in Step 3.
                _mapper.Map(updateSopServiceModel, existing);

                // 🎓 Server-set audit fields.
                existing.ModifiedAt = DateTime.UtcNow;

                // ── Step 3: DETACH ghost entities from change tracker ────
                // 🎓 CRITICAL: Must happen BEFORE any SaveChangesAsync call.
                // Without this, EF Core's DetectChanges would find the ghost
                // entities in the navigation property and INSERT them as new
                // rows — creating duplicate applicability rules.
                //
                // We snapshot the ghosts, clear the navigation, then set each
                // ghost's state to Detached so EF Core forgets about them.
                var ghostEntities = existing.SopApplicabilities.ToList();
                existing.SopApplicabilities.Clear();
                foreach (var ghost in ghostEntities)
                {
                    _apparelProDbContext.Entry(ghost).State = EntityState.Detached;
                }

                // ── Phase 1: Delete ORIGINAL applicability rules ─────────
                // 🎓 Using originalRules (real PKs captured in Step 1).
                // The ghost entities are already detached, so SaveChangesAsync
                // only processes the deletes — no surprise inserts.
                _apparelProDbContext.SopApplicabilities.RemoveRange(originalRules);
                await _apparelProDbContext.SaveChangesAsync();

                // ── Phase 2: Add new applicability rules ─────────────────
                // 🎓 Build fresh SopApplicability entities from the update
                // model's rule list. These are the ONLY entities that should
                // be inserted — with correct SopId and IsExcluded values.
                foreach (var rule in updateSopServiceModel.SopApplicabilities)
                {
                    existing.SopApplicabilities.Add(new SopApplicability
                    {
                        SopId = existing.SopId,
                        ApplicabilityType = rule.ApplicabilityType,
                        ApplicabilityKey = rule.ApplicabilityKey,
                        IsExcluded = rule.IsExcluded
                    });
                }

                await _apparelProDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteSopAsync(int sopId)
        {
            // 🎓 SOFT DELETE: Set IsActive = false rather than removing the record.
            // SOPs might be referenced in historical PDF reports, and we want to
            // keep the audit trail. Cascade delete on the SopApplicability FK means
            // if we ever DO hard-delete, children go with it.

            var existing = await _apparelProDbContext.StandardOperatingProcedures
                .FirstOrDefaultAsync(sop => sop.SopId == sopId);

            if (existing == null)
            {
                throw new InvalidOperationException($"SOP with ID {sopId} not found.");
            }

            existing.IsActive = false;
            existing.ModifiedAt = DateTime.UtcNow;

            await _apparelProDbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SopServiceModel>> GetActiveSopsForContextAsync(
            string? reportCode, string? buyerCode, string? supplierCode)
        {
            // ═══════════════════════════════════════════════════════════════
            // 🎓 THIS IS THE KEY QUERY FOR PDF INJECTION (Phase 2 Step 5).
            //
            // STRATEGY:
            // 1. Find all ACTIVE SOPs within their effective date range
            // 2. Join to SopApplicabilities to find INCLUDED rules
            // 3. Build context keys from the parameters (e.g., "ReportType:TrimSheet")
            // 4. Match inclusions: SOP is included if ANY of its rules match the context
            // 5. Filter exclusions: SOP is excluded if a specific exclusion rule matches
            // 6. Return included minus excluded, ordered by DisplayOrder
            //
            // 🎓 WHY TWO QUERIES INSTEAD OF ONE COMPLEX LINQ?
            // The inclusion + exclusion logic is clearer as two steps:
            //   Step A: Get all SOPs that have at least one matching inclusion rule
            //   Step B: From those, remove any that have a matching exclusion rule
            // This is easier to debug and maintain than a single mega-query.
            // ═══════════════════════════════════════════════════════════════

            var today = DateTime.UtcNow.Date;

            // 🎓 Build the set of context keys to match against.
            // Each parameter generates an (ApplicabilityType, ApplicabilityKey) pair.
            var contextKeys = new List<(string Type, string Key)>();
            contextKeys.Add(("All", "*")); // 🎓 Global SOPs always match

            if (!string.IsNullOrEmpty(reportCode))
                contextKeys.Add(("ReportType", reportCode));

            if (!string.IsNullOrEmpty(buyerCode))
                contextKeys.Add(("Buyer", buyerCode));

            if (!string.IsNullOrEmpty(supplierCode))
                contextKeys.Add(("Supplier", supplierCode));

            // 🎓 STEP A: Find all active, date-valid SOPs that have at least one
            // matching INCLUSION rule (IsExcluded = false).
            var includedSopIds = await _apparelProDbContext.SopApplicabilities
                .AsNoTracking()
                .Where(sa => !sa.IsExcluded
                    && contextKeys.Select(ck => ck.Type).Contains(sa.ApplicabilityType)
                    && contextKeys.Select(ck => ck.Key).Contains(sa.ApplicabilityKey))
                .Select(sa => sa.SopId)
                .Distinct()
                .ToListAsync();

            if (!includedSopIds.Any())
                return Enumerable.Empty<SopServiceModel>();

            // 🎓 STEP B: Find SOPs that have a matching EXCLUSION rule.
            // These override the inclusions for the specific context.
            var excludedSopIds = await _apparelProDbContext.SopApplicabilities
                .AsNoTracking()
                .Where(sa => sa.IsExcluded
                    && includedSopIds.Contains(sa.SopId)
                    && contextKeys.Select(ck => ck.Type).Contains(sa.ApplicabilityType)
                    && contextKeys.Select(ck => ck.Key).Contains(sa.ApplicabilityKey))
                .Select(sa => sa.SopId)
                .Distinct()
                .ToListAsync();

            // 🎓 STEP C: Load the final set of SOPs — included minus excluded,
            // filtered by active status and date validity.
            var finalSopIds = includedSopIds.Except(excludedSopIds).ToList();

            var entities = await _apparelProDbContext.StandardOperatingProcedures
                .AsNoTracking()
                .Where(sop => finalSopIds.Contains(sop.SopId)
                    && sop.IsActive
                    && sop.EffectiveFrom <= today
                    && (sop.EffectiveTo == null || sop.EffectiveTo >= today))
                .OrderBy(sop => sop.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SopServiceModel>>(entities);
        }
    }
}
