// ═══════════════════════════════════════════════════════════════════════════
//  IReportRegistryService.cs — Service Interface
//  Location: apparelPro.BusinessLogic/Services/IReportRegistryService.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 INTERFACE CONVENTION:
// Service interfaces live at:
//   apparelPro.BusinessLogic/Services/I{Entity}Service.cs
//
// They are NOT grouped by domain folder — they sit flat in the Services folder.
// This matches IBuyerService.cs, IStyleDetailsService.cs, etc.
//
// 🎓 DEPENDENCY INVERSION PRINCIPLE (SOLID's "D"):
// Controllers and other services depend on THIS interface, never on the
// concrete ReportRegistryService. This means:
//   • You can mock this in unit tests
//   • You can swap implementations without changing consumers
//   • The DI container resolves the concrete type at runtime
// ═══════════════════════════════════════════════════════════════════════════

using apparelPro.BusinessLogic.Services.Models.AI.IReportRegistryService;

namespace apparelPro.BusinessLogic.Services
{
    /// <summary>
    /// 🎓 Contract for accessing the report registry catalogue.
    ///
    /// Currently read-only — the registry is managed via SQL seed scripts
    /// and direct DB inserts, not through an API. CRUD methods can be added
    /// later if an admin UI for report management is needed.
    ///
    /// 🎓 PRIMARY CONSUMERS:
    ///   1. ReportRegistryController — exposes the catalogue via REST API
    ///   2. RagService (future enhancement) — reads active reports to inject
    ///      into Claude's system prompt for intent detection
    /// </summary>
    public interface IReportRegistryService
    {
        /// <summary>
        /// 🎓 Returns ALL active reports in display order.
        /// Used by the RAG pipeline to build the "available reports" context
        /// that gets injected into Claude's system prompt.
        ///
        /// Returns only IsActive == true reports, ordered by DisplayOrder.
        /// </summary>
        Task<IEnumerable<ReportRegistryServiceModel>> GetActiveReportsAsync();

        /// <summary>
        /// 🎓 Returns a SINGLE report by its code.
        /// Used by the backend when Claude detects a report intent —
        /// we look up the report code to get its EndpointTemplate and
        /// RequiredParams for PDF generation.
        ///
        /// Returns null if the report code doesn't exist or is inactive.
        /// </summary>
        Task<ReportRegistryServiceModel?> GetReportByCodeAsync(string reportCode);

        /// <summary>
        /// 🎓 Returns reports filtered by Category.
        /// Useful for admin screens that group reports by domain area
        /// (OrderManagement, Inventory, Shipments, etc.)
        /// </summary>
        Task<IEnumerable<ReportRegistryServiceModel>> GetReportsByCategoryAsync(string category);
    }
}
