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

                // 🎓 Map scalar properties from the update model onto the tracked entity.
                // AutoMapper's Map(source, destination) updates the destination in-place
                // without creating a new instance — preserving EF Core's change tracking.
                _mapper.Map(updateSopServiceModel, existing);

                // 🎓 Server-set audit fields.
                existing.ModifiedAt = DateTime.UtcNow;

                // 🎓 REPLACE-CHILDREN FIX — TWO-PHASE SAVE:
                // EF Core tracks new entities with temporary PKs (SopApplicabilityId = 0).
                // If we RemoveRange the old children AND add new children in the SAME
                // SaveChangesAsync call, EF gets confused — it tries to delete entities
                // that still have temporary keys, throwing:
                //   "The property 'SopApplicability.SopApplicabilityId' has a temporary
                //    value while attempting to change the entity's state to 'Deleted'."
                //
                // The fix: flush the deletes first (Phase 1), THEN add the new children
                // and save again (Phase 2). Both phases run inside the same transaction,
                // so if anything fails the entire operation rolls back atomically.

                // ── Phase 1: Delete existing applicability rules ──────────
                _apparelProDbContext.SopApplicabilities.RemoveRange(existing.SopApplicabilities);
                await _apparelProDbContext.SaveChangesAsync();

                // 🎓 Clear the navigation collection so EF Core doesn't hold
                // stale references to the just-deleted entities in memory.
                existing.SopApplicabilities.Clear();

                // ── Phase 2: Add new applicability rules ──────────────────
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
