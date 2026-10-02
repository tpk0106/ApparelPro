// ═══════════════════════════════════════════════════════════════════════════
//  SopServiceModel.cs — Business Layer Models
//  Location: apparelPro.BusinessLogic/Services/Models/AI/ISopService/SopServiceModel.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 SERVICE MODEL CONVENTION:
// Service models live under:
//   apparelPro.BusinessLogic/Services/Models/{DomainFolder}/I{Service}Service/
//
// The folder is named after the service INTERFACE (ISopService).
// This matches IReportRegistryService/ReportRegistryServiceModel.cs.
//
// 🎓 WHY THREE MODELS (Read, Create, Update)?
// Unlike ReportRegistry (read-only, seed-managed), SOPs are user-managed
// records with full CRUD. The 3-layer architecture requires:
//   • SopServiceModel — for read/list responses (includes PK and audit fields)
//   • CreateSopServiceModel — for create requests (no PK, no audit — server sets those)
//   • UpdateSopServiceModel — for update requests (includes PK for identification)
//
// 🎓 SopApplicabilityServiceModel:
// A flat model for the child applicability rules. Embedded as a collection
// in SopServiceModel and CreateSopServiceModel so the frontend can manage
// an SOP and its applicability rules in a single API call.
// ═══════════════════════════════════════════════════════════════════════════

namespace apparelPro.BusinessLogic.Services.Models.AI.ISopService
{
    /// <summary>
    /// 🎓 Business layer representation of SopApplicability.
    /// Used in both read and write operations — embedded as a child collection.
    /// </summary>
    public class SopApplicabilityServiceModel
    {
        public int SopApplicabilityId { get; set; }
        public int SopId { get; set; }
        public string ApplicabilityType { get; set; } = string.Empty;
        public string ApplicabilityKey { get; set; } = string.Empty;
        public bool IsExcluded { get; set; }
    }

    /// <summary>
    /// 🎓 Full read model — returned by GetSopsAsync, GetSopByIdAsync, etc.
    /// Includes PK, audit fields, and nested applicability rules.
    ///
    /// AutoMapper maps: StandardOperatingProcedure ↔ SopServiceModel
    /// </summary>
    public class SopServiceModel
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
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }

        // 🎓 Nested child collection — loaded via .Include() in the service layer.
        public ICollection<SopApplicabilityServiceModel> SopApplicabilities { get; set; } = new List<SopApplicabilityServiceModel>();
    }

    /// <summary>
    /// 🎓 Create model — used when adding a new SOP.
    /// No SopId (server generates it), no audit fields (server sets those).
    /// Includes applicability rules so admin can define the SOP + its rules
    /// in a single API call.
    ///
    /// AutoMapper maps: CreateSopServiceModel → StandardOperatingProcedure
    /// </summary>
    public class CreateSopServiceModel
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

        // 🎓 CreatedBy is set by the service from the JWT claim — not from the client.
        // But we need to pass it down from the controller → service, so it's here.
        public string CreatedBy { get; set; } = string.Empty;

        // 🎓 Applicability rules to create alongside the SOP.
        public ICollection<CreateSopApplicabilityServiceModel> SopApplicabilities { get; set; } = new List<CreateSopApplicabilityServiceModel>();
    }

    /// <summary>
    /// 🎓 Child create model — no PK fields (server generates them).
    /// </summary>
    public class CreateSopApplicabilityServiceModel
    {
        public string ApplicabilityType { get; set; } = string.Empty;
        public string ApplicabilityKey { get; set; } = string.Empty;
        public bool IsExcluded { get; set; }
    }

    /// <summary>
    /// 🎓 Update model — includes SopId for identification.
    /// The service layer uses this to find and update the existing record.
    ///
    /// AutoMapper maps: UpdateSopServiceModel → StandardOperatingProcedure
    /// </summary>
    public class UpdateSopServiceModel
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

        // 🎓 ModifiedBy is set by the service from the JWT claim.
        public string ModifiedBy { get; set; } = string.Empty;

        // 🎓 Full replacement of applicability rules on update.
        // The service layer deletes existing rules and re-creates from this list.
        // This is simpler than tracking individual adds/removes/updates and is
        // fine for the small number of rules per SOP (typically 1-5).
        public ICollection<CreateSopApplicabilityServiceModel> SopApplicabilities { get; set; } = new List<CreateSopApplicabilityServiceModel>();
    }
}
