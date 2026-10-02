// ═══════════════════════════════════════════════════════════════════════════
//  SopApplicability.cs — Data Entity
//  Location: ApparelPro.Data/Models/AI/SopApplicability.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS SopApplicability?
// This is the LINKING TABLE that controls WHERE each SOP appears.
// Instead of hard-coding "this SOP goes on Trim Sheet reports",
// we store linking rules so admins can:
//   1. Attach an SOP to a specific report type ("ReportType" + "TrimSheet")
//   2. Attach an SOP to ALL reports for a specific buyer ("Buyer" + "101")
//   3. Attach an SOP to a specific supplier ("Supplier" + "FABRIC-001")
//   4. Attach an SOP to an entity type ("EntityType" + "PurchaseOrder")
//   5. Attach an SOP globally to everything ("All" + "*")
//
// 🎓 HOW THE PDF ENGINE USES THIS:
// When generating a Trim Sheet PDF for buyer 101, the engine queries:
//   SELECT sop.* FROM StandardOperatingProcedures sop
//   JOIN SopApplicabilities sa ON sop.SopId = sa.SopId
//   WHERE sop.IsActive = 1
//     AND sop.EffectiveFrom <= @today
//     AND (sop.EffectiveTo IS NULL OR sop.EffectiveTo >= @today)
//     AND sa.IsExcluded = 0
//     AND (
//       (sa.ApplicabilityType = 'ReportType' AND sa.ApplicabilityKey = 'TrimSheet')
//       OR (sa.ApplicabilityType = 'Buyer' AND sa.ApplicabilityKey = '101')
//       OR (sa.ApplicabilityType = 'All' AND sa.ApplicabilityKey = '*')
//     )
//   ORDER BY sop.DisplayOrder
//
// 🎓 THE IsExcluded FLAG:
// Provides a NEGATIVE override. If an SOP applies to "All" + "*",
// but you want to EXCLUDE it from one specific buyer, add another
// SopApplicability row with IsExcluded = true:
//   { ApplicabilityType = "Buyer", ApplicabilityKey = "205", IsExcluded = true }
// The query engine checks exclusions AFTER inclusions and filters them out.
//
// 🎓 WHY NOT A SIMPLE FK TO Buyer/Supplier/ReportRegistry?
// Because the linking target varies — it can be a report code, a buyer code,
// a supplier code, or an entity type name. A polymorphic string key
// (ApplicabilityType + ApplicabilityKey) handles all cases without
// multiple nullable FK columns. The trade-off is no referential integrity
// at the DB level, but the admin UI validates these values at input time.
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.Data.Models.AI
{
    /// <summary>
    /// 🎓 Links a StandardOperatingProcedure to a specific context where it applies.
    ///
    /// ApplicabilityType + ApplicabilityKey together form the "WHERE does this SOP appear?" rule:
    ///   • "ReportType" + "TrimSheet" → appears on Trim Sheet reports
    ///   • "Buyer" + "101" → appears on any report for buyer 101
    ///   • "Supplier" + "FABRIC-001" → appears on any report involving this supplier
    ///   • "EntityType" + "PurchaseOrder" → appears on PO-related reports
    ///   • "All" + "*" → appears everywhere (global SOP)
    ///
    /// IsExcluded = true creates a negative override (exclude from this specific context).
    ///
    /// 🎓 ENTITY DESIGN:
    /// - int PK with identity (SopApplicabilityId)
    /// - FK to StandardOperatingProcedure (SopId)
    /// - Composite index on {SopId, ApplicabilityType, ApplicabilityKey} for fast lookups
    /// </summary>
    public class SopApplicability
    {
        // ── Primary Key ──────────────────────────────────────────────────
        public int SopApplicabilityId { get; set; }

        // ── Foreign Key ──────────────────────────────────────────────────
        // 🎓 Links back to the parent SOP. EF Core will create the FK
        // constraint via Fluent API in SopApplicabilityConfig.cs.
        public int SopId { get; set; }

        // ── Applicability Rule ───────────────────────────────────────────
        // 🎓 ApplicabilityType: The KIND of context this rule targets.
        //    Valid values: "ReportType", "EntityType", "Supplier", "Buyer", "All"
        //    Validated at the service layer, not by a DB constraint, because
        //    new types might be added as the system grows.
        //
        // 🎓 ApplicabilityKey: The SPECIFIC instance within that type.
        //    For "ReportType": matches ReportRegistry.ReportCode (e.g., "TrimSheet")
        //    For "Buyer": matches Buyer.BuyerCode as string (e.g., "101")
        //    For "Supplier": matches Supplier.SupplierCode as string (e.g., "FABRIC-001")
        //    For "EntityType": matches an entity type name (e.g., "PurchaseOrder")
        //    For "All": always "*" (wildcard — applies globally)
        public string ApplicabilityType { get; set; } = string.Empty;
        public string ApplicabilityKey { get; set; } = string.Empty;

        // ── Exclusion Override ────────────────────────────────────────────
        // 🎓 When false (default): this rule INCLUDES the SOP in the target context.
        // When true: this rule EXCLUDES the SOP from the target context.
        // Exclusions are processed AFTER inclusions — if an SOP matches both
        // an inclusion and an exclusion rule, the exclusion wins.
        //
        // Example: SOP applies to All/* (global), but excluded from Buyer/205.
        // This means every buyer except 205 sees this SOP on their reports.
        public bool IsExcluded { get; set; }

        // ── Navigation Property ──────────────────────────────────────────
        // 🎓 Back-reference to the parent SOP. Used for .Include() queries
        // and for EF Core's relationship configuration.
        public StandardOperatingProcedure StandardOperatingProcedure { get; set; } = null!;
    }
}
