// ═══════════════════════════════════════════════════════════════════════════
//  StandardOperatingProcedure.cs — Data Entity
//  Location: ApparelPro.Data/Models/AI/StandardOperatingProcedure.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS StandardOperatingProcedure (SOP)?
// This is the COMPANY RULES catalogue — Standard Operating Procedures that
// can be attached to reports, entities, suppliers, or buyers. SOPs appear:
//   1. On PDF reports as "Terms & Conditions" sections (Phase 2 Step 5)
//   2. In the RAG pipeline as searchable knowledge chunks (Phase 2 Step 6)
//   3. In a future admin UI for managing company procedures
//
// 🎓 HOW DOES AN SOP REACH A REPORT?
// Through the SopApplicability child table. When a Trim Sheet PDF is
// generated, the system queries SopApplicability for all SOPs where:
//   ApplicabilityType = "ReportType" AND ApplicabilityKey = "TrimSheet"
// Those SOP texts get injected into the PDF's Terms & Conditions section.
//
// 🎓 DESIGN DECISIONS:
// - int PK with identity (SopId) — unlike ReportRegistry's string PK,
//   SOPs are user-managed records where a meaningful code is secondary.
//   SopCode is a unique business identifier for lookups/references.
// - FullText is nvarchar(max) — SOP content can be long (paragraphs of
//   procedure text, legal terms, compliance requirements).
// - EffectiveFrom/EffectiveTo — date-bounded validity so SOPs can be
//   scheduled to take effect in the future or expire automatically.
// - Navigation property to SopApplicabilities — one SOP can have MANY
//   applicability rules (e.g., applies to "TrimSheet" report AND "Supplier:123").
//
// 🎓 NAMESPACE CONVENTION:
// Entities live under ApparelPro.Data.Models.{DomainFolder}
// We place this under "AI" since SOPs are consumed by the AI/RAG pipeline
// and the report generation subsystem.
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.Data.Models.AI
{
    /// <summary>
    /// 🎓 Represents a single Standard Operating Procedure that can be attached
    /// to reports, entity types, suppliers, or buyers via SopApplicability rules.
    ///
    /// Each row defines:
    ///   • A unique procedure (SopCode + Title)
    ///   • Its full text content (FullText — injected into PDFs and RAG)
    ///   • Categorisation and ordering (Category, DisplayOrder)
    ///   • Date-bounded validity (EffectiveFrom / EffectiveTo)
    ///   • Audit trail (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
    ///
    /// 🎓 ENTITY DESIGN:
    /// - No data annotations — all configuration via Fluent API in StandardOperatingProcedureConfig.cs
    /// - Nullable string? for optional fields, string.Empty default for required strings
    /// - ICollection navigation property for the one-to-many → SopApplicability
    /// </summary>
    public class StandardOperatingProcedure
    {
        // ── Primary Key ──────────────────────────────────────────────────
        // 🎓 Auto-increment int PK. SOPs are user-managed records, so a
        // surrogate key is more practical than a string PK. The SopCode
        // field below serves as the human-readable business identifier.
        public int SopId { get; set; }

        // ── Business Identifier ──────────────────────────────────────────
        // 🎓 SopCode: Unique, human-readable identifier like "SOP-TRIM-001".
        // Used in admin UIs, logs, and cross-references. Has a unique index
        // in the Fluent API config. Kept separate from the PK so the PK
        // stays stable even if the code needs to be renamed.
        public string SopCode { get; set; } = string.Empty;

        // ── Display & Content ────────────────────────────────────────────
        // 🎓 Title: Short descriptive name shown in admin lists and PDF headers.
        //    e.g. "Material Wastage Tolerance Policy"
        //
        // 🎓 Description: One-paragraph summary for admin UI tooltips and
        //    RAG context. Helps Claude understand WHAT the SOP covers when
        //    deciding whether to include it in a response.
        //
        // 🎓 FullText: The complete procedure text — nvarchar(max) because
        //    SOPs can be multiple paragraphs of legal/compliance text.
        //    This is what gets injected into PDF "Terms & Conditions" sections
        //    and embedded into Qdrant as searchable RAG chunks.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;

        // ── Classification ───────────────────────────────────────────────
        // 🎓 Category groups SOPs by domain area for admin filtering.
        //    e.g. "Quality", "Procurement", "Shipping", "Finance", "General"
        public string Category { get; set; } = string.Empty;

        // ── Control Flags ────────────────────────────────────────────────
        // 🎓 IsActive: Soft-enable/disable without deletion.
        //    Set to false to temporarily hide an SOP from reports and RAG
        //    without losing the record (e.g., while under legal review).
        public bool IsActive { get; set; } = true;

        // 🎓 DisplayOrder: Controls the sequence when multiple SOPs appear
        //    on the same PDF report. Lower numbers print first.
        public int DisplayOrder { get; set; }

        // ── Date-Bounded Validity ────────────────────────────────────────
        // 🎓 EffectiveFrom: The date this SOP takes effect.
        //    SOPs with EffectiveFrom in the future won't appear on reports yet.
        //
        // 🎓 EffectiveTo: The date this SOP expires (nullable = no expiry).
        //    Expired SOPs are automatically excluded from report injection
        //    and RAG embedding without manual deactivation.
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // ── Audit Trail ──────────────────────────────────────────────────
        // 🎓 CreatedBy / ModifiedBy: Populated from the JWT token's
        //    ClaimTypes.Name claim at the controller level.
        //    CreatedAt / ModifiedAt: UTC timestamps set by the service layer.
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }

        // ── Navigation Property ──────────────────────────────────────────
        // 🎓 One SOP → Many SopApplicability rules.
        // Tells EF Core about the parent-child relationship.
        // Used for .Include() when loading an SOP with its applicability rules.
        public ICollection<SopApplicability> SopApplicabilities { get; set; } = new List<SopApplicability>();
    }
}
