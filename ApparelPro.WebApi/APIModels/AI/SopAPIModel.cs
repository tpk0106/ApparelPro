// ═══════════════════════════════════════════════════════════════════════════
//  SopAPIModel.cs — Controller Layer Models
//  Location: ApparelPro.WebApi/APIModels/AI/SopAPIModel.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS THIS FILE?
// API models for the SOP Admin CRUD feature. These are the OUTERMOST layer —
// what the React frontend sends and receives as JSON.
//
// 🎓 WHY SEPARATE FROM SERVICE MODELS?
// The 3-tier architecture requires separate models at each boundary:
//   Controller Layer (this file) → Service Layer → Data Layer (EF Core)
//
// API models:
//   • EXPOSE only what the frontend needs (no internal IDs the client shouldn't set)
//   • HIDE backend implementation details
//   • Are the JSON contract the frontend depends on
//
// 🎓 MODEL NAMING CONVENTION (from clipper-migration skill):
//   • [Name]APIModel — read/response model
//   • Create[Name]APIModel — create request model
//   • Update[Name]APIModel — update request model
//
// 🎓 AUTOMAPPER MAPPINGS (already wired in ServicetoAPIModelMappings.cs):
//   CreateMap<SopServiceModel, SopAPIModel>().MaxDepth(2).ReverseMap();
//   CreateMap<CreateSopAPIModel, CreateSopServiceModel>().MaxDepth(2);
//   CreateMap<UpdateSopAPIModel, UpdateSopServiceModel>().MaxDepth(2);
//   CreateMap<SopApplicabilityServiceModel, SopApplicabilityAPIModel>().MaxDepth(2).ReverseMap();
//   CreateMap<CreateSopApplicabilityAPIModel, CreateSopApplicabilityServiceModel>().MaxDepth(2);
//   CreateMap<PaginationResult<SopServiceModel>, PaginationAPIModel<SopAPIModel>>()
//       .ConvertUsing<PaginationResultToPaginationAPITypeConverter<...>>();
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.WebApi.APIModels.AI
{
    // ─────────────────────────────────────────────────────────────────────
    //  READ MODELS (returned in GET responses)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Full read model — returned by GET /api/sop/list and GET /api/sop/{id}.
    /// Includes PK, all user-facing fields, and nested applicability rules.
    ///
    /// 🎓 FIELDS EXPOSED:
    /// All fields are exposed to the frontend because the admin UI needs to:
    ///   • Display all SOP details in the table
    ///   • Pre-fill the edit dialog with current values
    ///   • Show audit trail (CreatedBy, ModifiedAt, etc.)
    ///
    /// 🎓 WHY INCLUDE AUDIT FIELDS?
    /// Unlike ReportRegistryAPIModel (which hides internal fields), SOP audit
    /// fields are user-facing: the admin needs to see WHO created/modified an SOP
    /// and WHEN, for compliance tracking.
    /// </summary>
    public class SopAPIModel
    {
        public int SopId { get; set; }
        public string SopCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // 🎓 Audit fields — read-only on the client side.
        // The frontend displays these but never sends them in create/update requests.
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }

        // 🎓 Nested child collection — the applicability rules for this SOP.
        // AutoMapper maps SopApplicabilityServiceModel → SopApplicabilityAPIModel
        // automatically because .MaxDepth(2) allows one level of nesting.
        public ICollection<SopApplicabilityAPIModel> SopApplicabilities { get; set; } = new List<SopApplicabilityAPIModel>();
    }

    /// <summary>
    /// 🎓 Read model for an applicability rule — nested inside SopAPIModel.
    ///
    /// Shows the rule's PK (SopApplicabilityId) so the frontend can reference
    /// specific rules, and the parent FK (SopId) for completeness.
    /// </summary>
    public class SopApplicabilityAPIModel
    {
        public int SopApplicabilityId { get; set; }
        public int SopId { get; set; }
        public string ApplicabilityType { get; set; } = string.Empty;
        public string ApplicabilityKey { get; set; } = string.Empty;
        public bool IsExcluded { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CREATE MODELS (sent in POST requests)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Create request model — POST /api/sop
    ///
    /// 🎓 FIELDS EXCLUDED (server sets these):
    ///   • SopId — auto-incremented by SQL Server
    ///   • CreatedBy — extracted from JWT claim in the controller
    ///   • CreatedAt — set by the service layer (DateTime.UtcNow)
    ///   • ModifiedBy / ModifiedAt — not applicable on create
    ///
    /// 🎓 INCLUDES APPLICABILITY RULES:
    /// The frontend sends the SOP + its rules in a single POST, so the admin
    /// can define everything in one dialog submission. The service layer
    /// wraps both inserts in a transaction.
    /// </summary>
    public class CreateSopAPIModel
    {
        public string SopCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // 🎓 Applicability rules to create alongside the SOP.
        // Uses CreateSopApplicabilityAPIModel (no PKs — server assigns them).
        public ICollection<CreateSopApplicabilityAPIModel> SopApplicabilities { get; set; } = new List<CreateSopApplicabilityAPIModel>();
    }

    /// <summary>
    /// 🎓 Child create model for applicability rules.
    ///
    /// Only three fields — the minimum needed to define a rule:
    ///   • ApplicabilityType — "All", "Buyer", "Supplier", "ReportType", "EntityType"
    ///   • ApplicabilityKey — "*", "5", "TrimSheet", etc.
    ///   • IsExcluded — false for inclusion, true for exclusion override
    ///
    /// The SopApplicabilityId and SopId are set by the server.
    /// </summary>
    public class CreateSopApplicabilityAPIModel
    {
        public string ApplicabilityType { get; set; } = string.Empty;
        public string ApplicabilityKey { get; set; } = string.Empty;
        public bool IsExcluded { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UPDATE MODELS (sent in PUT requests)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🎓 Update request model — PUT /api/sop
    ///
    /// 🎓 INCLUDES SopId:
    /// Unlike CreateSopAPIModel, the update model includes SopId so the
    /// service layer knows WHICH SOP to update. The controller validates
    /// that this SOP exists before proceeding.
    ///
    /// 🎓 FIELDS EXCLUDED (server sets these):
    ///   • ModifiedBy — extracted from JWT claim in the controller
    ///   • ModifiedAt — set by the service layer
    ///   • CreatedBy / CreatedAt — immutable after creation
    ///
    /// 🎓 REPLACE-CHILDREN PATTERN:
    /// The SopApplicabilities collection is a FULL REPLACEMENT — the service
    /// layer deletes ALL existing rules for this SOP and re-creates them
    /// from this list. This is simpler than tracking individual adds/removes
    /// and works well for the small number of rules per SOP (typically 1-5).
    /// </summary>
    public class UpdateSopAPIModel
    {
        public int SopId { get; set; }
        public string SopCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // 🎓 Full replacement of applicability rules.
        // Uses the same CreateSopApplicabilityAPIModel as the create flow
        // (no PKs needed — old rules are deleted, new ones get fresh PKs).
        public ICollection<CreateSopApplicabilityAPIModel> SopApplicabilities { get; set; } = new List<CreateSopApplicabilityAPIModel>();
    }
}
