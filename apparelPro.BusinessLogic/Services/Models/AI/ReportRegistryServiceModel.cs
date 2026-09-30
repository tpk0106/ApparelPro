// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistryServiceModel.cs — Business Layer Model
//  Location: apparelPro.BusinessLogic/Services/Models/AI/IReportRegistryService/ReportRegistryServiceModel.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 SERVICE MODEL CONVENTION:
// Service models live under:
//   apparelPro.BusinessLogic/Services/Models/{DomainFolder}/I{Service}Service/
//
// The folder is named after the service INTERFACE (IReportRegistryService).
// This is an existing project convention — see IStyleDetailsService folder
// containing StyleDetailsServiceModel.cs.
//
// 🎓 WHY A SEPARATE MODEL FROM THE ENTITY?
// Clean Architecture principle — the business layer should NOT expose
// EF Core entities to consumers. The service model:
//   • Shields the controller from database schema changes
//   • Can include computed/derived fields not in the DB
//   • Gets mapped via AutoMapper (Entity ↔ ServiceModel)
//
// 🎓 FOR ReportRegistry, WE ONLY NEED ONE MODEL (not Create/Update):
// This is a seed-data table managed by developers/DBAs, not end users.
// The RAG pipeline only READS from it. No CRUD endpoints needed initially.
// If an admin UI is added later, we'll add Create/Update models then.
// ═══════════════════════════════════════════════════════════════════════════

namespace apparelPro.BusinessLogic.Services.Models.AI.IReportRegistryService
{
    /// <summary>
    /// 🎓 Business layer representation of a ReportRegistry entry.
    ///
    /// Used by:
    ///   • ReportRegistryService — to return report catalogue data
    ///   • RagService (future) — to inject available reports into Claude's prompt
    ///   • ReportRegistryController — mapped to ReportRegistryAPIModel for API responses
    ///
    /// All properties mirror the entity. AutoMapper handles the 1:1 mapping.
    /// </summary>
    public class ReportRegistryServiceModel
    {
        public string ReportCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RequiredParams { get; set; } = "[]";
        public string? ParamSources { get; set; }
        public string EndpointTemplate { get; set; } = string.Empty;
        public string? CommonNames { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
