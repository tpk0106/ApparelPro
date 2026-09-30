// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistryAPIModel.cs — Controller Layer Model
//  Location: ApparelPro.WebApi/APIModels/AI/ReportRegistryAPIModel.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 API MODEL CONVENTION:
// API models live under:
//   ApparelPro.WebApi/APIModels/{DomainFolder}/
//
// They are the OUTERMOST layer — what the frontend receives as JSON.
// This model is a subset of the ServiceModel, exposing only what
// the frontend needs. Internal fields like EndpointTemplate and
// ParamSources are NOT exposed to the client.
//
// 🎓 WHY HIDE EndpointTemplate AND ParamSources?
// These are backend implementation details:
//   • EndpointTemplate: The internal API route — exposing this leaks
//     your API structure to the client
//   • ParamSources: Backend resolution strategy — the frontend doesn't
//     need to know how parameters are extracted
//
// The frontend only needs to know: "what reports exist, what they're called,
// and what category they're in" — for display purposes and potentially
// a report picker UI in the future.
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.WebApi.APIModels.AI
{
    /// <summary>
    /// 🎓 API-facing representation of a report registry entry.
    ///
    /// Exposes only the fields the frontend needs:
    ///   • ReportCode — unique identifier (used in future report request APIs)
    ///   • DisplayName — human-readable name for UI display
    ///   • Description — tooltip/detail text
    ///   • Category — for grouping in menus
    ///   • CommonNames — alternative names (could be used for client-side search)
    ///   • IsActive, DisplayOrder — for UI rendering control
    ///
    /// Fields intentionally excluded (backend-only):
    ///   • RequiredParams — internal to the RAG pipeline
    ///   • ParamSources — internal parameter resolution strategy
    ///   • EndpointTemplate — internal API routing
    /// </summary>
    public class ReportRegistryAPIModel
    {
        public string ReportCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? CommonNames { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
