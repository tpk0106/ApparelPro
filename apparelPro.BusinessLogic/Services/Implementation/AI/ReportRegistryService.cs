// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistryService.cs — Service Implementation
//  Location: apparelPro.BusinessLogic/Services/Implementation/AI/ReportRegistryService.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 SERVICE IMPLEMENTATION CONVENTION:
// Implementations live under:
//   apparelPro.BusinessLogic/Services/Implementation/{DomainFolder}/
//
// This matches BuyerService.cs under Implementation/Reference/,
// and StyleDetailsService.cs under Implementation/OrderManagement/.
//
// 🎓 CONSTRUCTOR PATTERN:
// Every service follows this exact constructor pattern:
//   1. Inject IMapper and ApparelProDbContext (mandatory for all services)
//   2. Explicit null checks using if-throw (not ?? throw)
//   3. Private readonly fields with underscore prefix
//
// 🎓 QUERY PATTERN:
// All read operations use .AsNoTracking() for performance —
// we're not modifying entities, just reading them.
// ═══════════════════════════════════════════════════════════════════════════

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ApparelPro.Data;
using apparelPro.BusinessLogic.Services.Models.AI.IReportRegistryService;

namespace apparelPro.BusinessLogic.Services.Implementation.AI
{
    /// <summary>
    /// 🎓 Concrete implementation of IReportRegistryService.
    ///
    /// This service provides READ access to the report registry catalogue.
    /// It's intentionally simple — no caching, no pagination, no filtering —
    /// because the registry is small (typically 10-30 rows) and read infrequently
    /// (once per RAG query to build the prompt context).
    ///
    /// 🎓 LIFETIME: Registered as TRANSIENT in DI.
    /// Follows the same pattern as BuyerService and other reference services.
    /// Transient is fine because:
    ///   • No expensive state to initialise
    ///   • ApparelProDbContext is scoped (one per HTTP request)
    ///   • Transient service can safely consume scoped dependencies
    /// </summary>
    public class ReportRegistryService : IReportRegistryService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        /// <summary>
        /// 🎓 CONSTRUCTOR INJECTION:
        /// Follows the exact same pattern as BuyerService:
        ///   • IMapper for Entity ↔ ServiceModel conversions
        ///   • ApparelProDbContext for database access
        ///   • Explicit null checks (project convention — uses if-throw, not ?? throw)
        /// </summary>
        public ReportRegistryService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            if (apparelProDbContext == null) throw new ArgumentNullException(nameof(apparelProDbContext));

            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ReportRegistryServiceModel>> GetActiveReportsAsync()
        {
            // 🎓 QUERY PATTERN:
            // 1. Access the DbSet via _apparelProDbContext
            // 2. .AsNoTracking() — we're only reading, no change tracking needed
            // 3. .Where() for filtering
            // 4. .OrderBy() for consistent ordering
            // 5. .ToListAsync() to execute the query
            // 6. _mapper.Map<>() to convert entities to service models

            var entities = await _apparelProDbContext.ReportRegistries
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReportRegistryServiceModel>>(entities);
        }

        /// <inheritdoc />
        public async Task<ReportRegistryServiceModel?> GetReportByCodeAsync(string reportCode)
        {
            // 🎓 SingleOrDefaultAsync vs FirstOrDefaultAsync:
            // We use FirstOrDefaultAsync here — ReportCode is a PK so there's
            // only ever one match, but FirstOrDefault is slightly more efficient
            // (stops at the first match, doesn't verify uniqueness at the DB level).
            // This matches the pattern used in other services.

            var entity = await _apparelProDbContext.ReportRegistries
                .AsNoTracking()
                .Where(r => r.ReportCode == reportCode && r.IsActive)
                .FirstOrDefaultAsync();

            return entity == null ? null : _mapper.Map<ReportRegistryServiceModel>(entity);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ReportRegistryServiceModel>> GetReportsByCategoryAsync(string category)
        {
            var entities = await _apparelProDbContext.ReportRegistries
                .AsNoTracking()
                .Where(r => r.Category == category && r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReportRegistryServiceModel>>(entities);
        }
    }
}
