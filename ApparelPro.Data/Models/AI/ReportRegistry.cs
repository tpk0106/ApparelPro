// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistry.cs — Data Entity
//  Location: ApparelPro.Data/Models/AI/ReportRegistry.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHAT IS ReportRegistry?
// This is the SYSTEM CATALOGUE of reports that the RAG pipeline can generate.
// Instead of hard-coding report names in prompts, we store them in the database
// so that:
//   1. Claude knows WHAT reports exist (injected into the RAG system prompt)
//   2. Claude knows WHAT PARAMETERS each report needs (RequiredParams)
//   3. Claude knows the COMMON NAMES users might call each report (CommonNames)
//   4. Admins can add new reports without code changes (just a DB row)
//
// 🎓 WHY NOT A SYNONYM TABLE?
// Power Platform uses keyword-matching synonym tables because its NLU is limited.
// Claude already understands that "material consumption" = "trim sheet" = "BOM report".
// What Claude DOESN'T know is what reports YOUR system offers.
// So we teach Claude our catalogue, not our language.
//
// 🎓 THE CommonNames FIELD:
// This is NOT a matching engine — it's CONTEXT for Claude's prompt.
// When injected, Claude sees:
//   "Trim Sheet Report (also known as: trim sheet, material consumption,
//    BOM report, costing sheet, trim costing)"
// Claude's own NLU does the fuzzy matching from there.
//
// 🎓 NAMESPACE CONVENTION:
// Entities live under ApparelPro.Data.Models.{DomainFolder}
// We place this under "AI" since it's part of the AI/RAG subsystem.
// ═══════════════════════════════════════════════════════════════════════════

namespace ApparelPro.Data.Models.AI
{
    /// <summary>
    /// 🎓 Represents a single report that the RAG pipeline can identify and generate.
    ///
    /// Each row tells Claude:
    ///   • The report exists (ReportCode + DisplayName)
    ///   • What it does (Description — helps Claude disambiguate "consumption" in different contexts)
    ///   • What parameters it needs (RequiredParams — JSON array of parameter names)
    ///   • Where those parameters come from (ParamSources — JSON describing resolution strategy)
    ///   • What users might call it (CommonNames — comma-separated aliases for the prompt)
    ///   • Which API endpoint generates it (EndpointTemplate)
    ///
    /// 🎓 ENTITY DESIGN:
    /// - No data annotations — all configuration via Fluent API in ReportRegistryConfig.cs
    /// - Nullable string? for optional fields, string.Empty default for required strings
    /// - Follows the same patterns as Buyer, Style, and other reference entities
    /// </summary>
    public class ReportRegistry
    {
        // ── Primary Key ──────────────────────────────────────────────────
        // 🎓 Using a string PK (not auto-increment int) because report codes
        // are meaningful business identifiers like "TrimSheet", "SupplierPO".
        // This makes seed scripts readable and JOIN/WHERE clauses self-documenting.
        public string ReportCode { get; set; } = string.Empty;

        // ── Display & Description ────────────────────────────────────────
        // 🎓 DisplayName: Human-readable name shown in UI lists and logs.
        // 🎓 Description: Longer text that helps Claude disambiguate context.
        //    e.g. "Material consumption & costing breakdown for a style"
        //    When the user says "consumption", Claude reads this description
        //    to decide if they mean material consumption (Trim Sheet) or
        //    stock consumption (Inventory report) or budget consumption (Finance).
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // ── Classification ───────────────────────────────────────────────
        // 🎓 Category groups reports by domain area.
        // Maps to the existing navigation structure:
        //   "OrderManagement" → Reports → Order Management
        //   "Inventory" → Reports → Inventory
        //   "Shipments" → Reports → Shipments
        // Claude uses this for additional context when disambiguating.
        public string Category { get; set; } = string.Empty;

        // ── Parameter Metadata (JSON) ────────────────────────────────────
        // 🎓 RequiredParams: JSON array of parameter names the report endpoint needs.
        //    Example: ["buyerCode","order","typeCode","styleCode"]
        //    Claude uses this to know WHAT to extract from the RAG chunks.
        //
        // 🎓 ParamSources: JSON object describing WHERE each parameter comes from.
        //    Example: {
        //      "buyerCode": { "entity": "Style", "field": "BuyerCode", "type": "int" },
        //      "order": { "entity": "Style", "field": "Order", "type": "string" },
        //      "typeCode": { "entity": "Style", "field": "TypeCode", "type": "int" },
        //      "styleCode": { "entity": "Style", "field": "StyleCode", "type": "string" }
        //    }
        //    This tells the backend HOW to resolve parameters from RAG search results.
        public string RequiredParams { get; set; } = "[]";
        public string? ParamSources { get; set; }

        // ── API Endpoint ─────────────────────────────────────────────────
        // 🎓 The relative API path that generates this report.
        // For Trim Sheet: "api/trim-sheet-report/pdf"
        // The backend calls this internally when RAG detects a report intent.
        public string EndpointTemplate { get; set; } = string.Empty;

        // ── Common Names (Synonym Context for Claude) ────────────────────
        // 🎓 THIS IS THE KEY FIELD for intent recognition.
        // Comma-separated list of alternative names users might use.
        // NOT a keyword-matching engine — injected into Claude's system prompt
        // so Claude can use its own NLU to match fuzzy user phrases.
        //
        // Example: "trim sheet, material consumption, BOM report, costing sheet,
        //           trim costing, material cost breakdown"
        //
        // 🎓 WHEN TO PROMOTE TO A CHILD TABLE:
        // Start with this flat field. When you have 20+ reports and users hit
        // edge cases Claude gets wrong, THEN create a ReportRegistrySynonym
        // child table. Don't over-engineer day one.
        public string? CommonNames { get; set; }

        // ── Control Flags ────────────────────────────────────────────────
        // 🎓 IsActive: Soft-enable/disable without code deployment.
        //    Set to false to temporarily hide a report from the RAG pipeline
        //    (e.g. while the endpoint is being refactored).
        public bool IsActive { get; set; } = true;

        // 🎓 DisplayOrder: Controls the sequence when listing available reports
        //    in admin screens or when injecting into Claude's prompt.
        //    Lower numbers appear first.
        public int DisplayOrder { get; set; }
    }
}
