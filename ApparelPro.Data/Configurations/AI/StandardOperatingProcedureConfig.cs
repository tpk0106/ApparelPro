// ═══════════════════════════════════════════════════════════════════════════
//  StandardOperatingProcedureConfig.cs — EF Core Fluent API Configuration
//  Location: ApparelPro.Data/Configurations/AI/StandardOperatingProcedureConfig.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHY FLUENT API INSTEAD OF DATA ANNOTATIONS?
// ApparelPro uses Fluent API exclusively for ALL entity configurations.
// This keeps the entity classes clean (pure POCOs) and centralises all
// database mapping in one place per entity.
//
// 🎓 NAMING CONVENTION:
// Config classes follow the pattern: {Entity}Config
// (NOT {Entity}Configuration — see ReportRegistryConfig.cs for precedent)
// Namespace: ApparelPro.Data.Configurations.AI
//
// 🎓 TABLE NAME:
// Per the project's pluralisation rule, the entity "StandardOperatingProcedure"
// maps to the table "StandardOperatingProcedures" (plural).
//
// 🎓 SEED DATA (HasData):
// This config seeds sample SOPs covering the main categories used in
// garment manufacturing: Quality, Procurement, Shipping, and General.
// These provide initial RAG knowledge for the AI pipeline and sample
// data for the SOP Admin UI to display.
//
// 🎓 IMPORTANT — SEED PK VALUES:
// HasData() requires explicit PK values. We start SopId at 1 and increment.
// EF Core tracks these by PK — changing a seeded PK creates a NEW row
// instead of updating the old one. If you add more seeds later, continue
// from the next available SopId.
// ═══════════════════════════════════════════════════════════════════════════

using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI
{
    public class StandardOperatingProcedureConfig : IEntityTypeConfiguration<StandardOperatingProcedure>
    {
        public void Configure(EntityTypeBuilder<StandardOperatingProcedure> entity)
        {
            // ── Table Mapping ────────────────────────────────────────────
            // 🎓 Explicit table name ensures EF Core doesn't rely on DbSet
            // property naming conventions. Plural form per project rule.
            entity.ToTable("StandardOperatingProcedures");

            // ── Primary Key ──────────────────────────────────────────────
            // 🎓 Auto-increment int PK. Unlike ReportRegistry's string PK,
            // SOPs are user-managed records where a surrogate key is more
            // practical. SopCode below serves as the human-readable ID.
            entity.HasKey(k => k.SopId);

            entity.Property(p => p.SopId)
                .UseIdentityColumn();

            // ── Column Configurations ────────────────────────────────────
            // 🎓 Every property gets explicit HasColumnType() and HasMaxLength()
            // per the project convention (see BuyerConfig.cs, ReportRegistryConfig.cs).

            // 🎓 SopCode: Unique business identifier like "SOP-TRIM-001".
            // Has a unique index for fast lookups and to prevent duplicates.
            entity.Property(p => p.SopCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar");

            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar");

            // 🎓 FullText: nvarchar(max) — SOP content can be long paragraphs
            // of procedure text, legal terms, compliance requirements.
            // This is what gets injected into PDFs and embedded into Qdrant
            // as searchable RAG chunks.
            entity.Property(p => p.FullText)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar");

            entity.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnType("bit");

            entity.Property(p => p.DisplayOrder)
                .IsRequired()
                .HasColumnType("int");

            // 🎓 EffectiveFrom: Required — every SOP must have a start date.
            // EffectiveTo: Nullable — null means "no expiry" (indefinite validity).
            entity.Property(p => p.EffectiveFrom)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(p => p.EffectiveTo)
                .IsRequired(false)
                .HasColumnType("datetime2");

            // ── Audit Fields ─────────────────────────────────────────────
            // 🎓 CreatedBy/ModifiedBy: Populated from JWT token's ClaimTypes.Name.
            // CreatedAt/ModifiedAt: UTC timestamps set by the service layer.
            entity.Property(p => p.CreatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar");

            entity.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(p => p.ModifiedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar");

            entity.Property(p => p.ModifiedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            // ── Indexes ──────────────────────────────────────────────────
            // 🎓 Unique index on SopCode — prevents duplicate business identifiers
            // and enables fast lookups by code.
            entity.HasIndex(p => p.SopCode)
                .IsUnique();

            // 🎓 Index on Category — used for admin UI filtering ("show Quality SOPs").
            entity.HasIndex(p => p.Category);

            // 🎓 Index on IsActive — common query pattern: "get all active SOPs".
            entity.HasIndex(p => p.IsActive);

            // 🎓 Composite index on EffectiveFrom + EffectiveTo — the PDF engine
            // and RAG pipeline both filter SOPs by date validity range.
            entity.HasIndex(p => new { p.EffectiveFrom, p.EffectiveTo });

            // ── Relationship ─────────────────────────────────────────────
            // 🎓 One SOP → Many SopApplicability rules.
            // Cascade delete: when an SOP is deleted, all its applicability
            // rules are automatically removed (they have no meaning without
            // the parent SOP).
            entity.HasMany(e => e.SopApplicabilities)
                .WithOne(a => a.StandardOperatingProcedure)
                .HasForeignKey(a => a.SopId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Seed Data ────────────────────────────────────────────────
            // 🎓 HasData() bakes these rows into the EF migration.
            // When you run "dotnet ef migrations add SeedSopData", EF will
            // generate InsertData() calls in the migration's Up() method.
            //
            // 🎓 CATEGORIES COVERED:
            //   1. Quality    — fabric inspection, garment measurement tolerance
            //   2. Procurement — material wastage, supplier lead time
            //   3. Shipping   — packing, export documentation
            //   4. General    — global company policy applied to everything
            //
            // 🎓 NOTE: The SopApplicability seed rows linking these SOPs to
            // specific contexts are in SopApplicabilityConfig.cs.

            entity.HasData(

                // ─────────────────────────────────────────────────────────
                // SOP 1: Quality — Fabric Inspection
                // 🎓 Covers the standard AQL inspection procedure for incoming
                // fabric rolls. Attached to TrimSheet reports and Buyer-specific
                // contexts via SopApplicability seeds.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 1,
                    SopCode = "SOP-QC-001",
                    Title = "Fabric Inspection — AQL 2.5 Four-Point System",
                    Description = "Standard inspection procedure for incoming fabric rolls using the four-point grading system at AQL 2.5 acceptance level.",
                    FullText = "1. SCOPE: This procedure applies to all incoming fabric rolls received at the warehouse before cutting.\n\n" +
                               "2. SAMPLING: Inspect a minimum of 10% of total rolls per delivery, selected randomly across all colour lots. For orders under 500 yards, inspect 100% of rolls.\n\n" +
                               "3. FOUR-POINT GRADING:\n" +
                               "   - Defects up to 3 inches: 1 point\n" +
                               "   - Defects 3–6 inches: 2 points\n" +
                               "   - Defects 6–9 inches: 3 points\n" +
                               "   - Defects over 9 inches: 4 points\n\n" +
                               "4. ACCEPTANCE: Total points per 100 linear yards must not exceed 40 points. Rolls exceeding this threshold are flagged for return or downgrading.\n\n" +
                               "5. DOCUMENTATION: Record all inspection results in the Fabric Inspection Log with roll number, colour lot, defect count, and total penalty points. Attach photographs of major defects.\n\n" +
                               "6. ESCALATION: If more than 20% of inspected rolls fail, notify the Procurement Manager and issue a Supplier Non-Conformance Report (NCR) within 24 hours.",
                    Category = "Quality",
                    IsActive = true,
                    DisplayOrder = 1,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 2: Quality — Garment Measurement Tolerance
                // 🎓 Defines acceptable measurement deviations at each
                // production stage. Critical for buyer compliance.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 2,
                    SopCode = "SOP-QC-002",
                    Title = "Garment Measurement Tolerance — Production Stages",
                    Description = "Acceptable measurement deviation limits at cutting, sewing, and finishing stages for all garment types.",
                    FullText = "1. SCOPE: Applicable to all garment types across cutting, sewing, and finishing stages.\n\n" +
                               "2. CUTTING TOLERANCE:\n" +
                               "   - Critical measurements (chest, waist, hip): ±¼ inch (±6mm)\n" +
                               "   - Non-critical measurements (sleeve opening, hem width): ±⅜ inch (±10mm)\n\n" +
                               "3. SEWING TOLERANCE:\n" +
                               "   - Seam allowance: ±1/16 inch (±2mm) from spec\n" +
                               "   - Stitch count: 10–12 stitches per inch unless buyer spec states otherwise\n" +
                               "   - Critical points (collar, cuff): ±⅛ inch (±3mm)\n\n" +
                               "4. FINISHING TOLERANCE:\n" +
                               "   - After wash/press: overall ±½ inch (±13mm) from buyer spec\n" +
                               "   - Shrinkage allowance must be pre-calculated per fabric test report\n\n" +
                               "5. AUDIT: Measure 5 garments per size per colour lot. All 5 must pass for the lot to proceed. If any single garment fails by more than double the tolerance, quarantine the entire lot.\n\n" +
                               "6. RECORDS: File measurement audit sheets with the production batch record. Retain for 12 months post-shipment.",
                    Category = "Quality",
                    IsActive = true,
                    DisplayOrder = 2,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 3: Procurement — Material Wastage Allowance
                // 🎓 Defines the maximum wastage percentages for different
                // material types when calculating consumption quantities.
                // Directly relevant to Trim Sheet cost calculations.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 3,
                    SopCode = "SOP-PROC-001",
                    Title = "Material Wastage Allowance Policy",
                    Description = "Maximum allowable wastage percentages by material type for consumption and costing calculations.",
                    FullText = "1. SCOPE: This policy governs the wastage factor applied when calculating material requirements for purchase orders and costing sheets.\n\n" +
                               "2. STANDARD WASTAGE ALLOWANCES:\n" +
                               "   - Shell fabric (woven): 5% of net consumption\n" +
                               "   - Shell fabric (knit): 8% of net consumption\n" +
                               "   - Lining fabric: 3% of net consumption\n" +
                               "   - Interlining / Fusible: 5% of net consumption\n" +
                               "   - Trims (buttons, zippers, labels): 2% of net consumption\n" +
                               "   - Thread: 10% of net consumption\n" +
                               "   - Elastic / Drawcord: 3% of net consumption\n\n" +
                               "3. OVERRIDES: Buyer-specific wastage factors take precedence if explicitly agreed in the order contract. Document any override in the style master's notes field.\n\n" +
                               "4. CALCULATION: Gross requirement = Net consumption × (1 + wastage%). This gross figure is what appears on purchase orders and supplier RFQs.\n\n" +
                               "5. REVIEW: Wastage allowances are reviewed quarterly against actual production waste data. If actual waste exceeds the allowance by more than 1% for two consecutive quarters, escalate to the Production Manager for root cause analysis.",
                    Category = "Procurement",
                    IsActive = true,
                    DisplayOrder = 3,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 4: Procurement — Supplier Lead Time Requirements
                // 🎓 Standard lead times by material category. Used by
                // the planning module and as RAG knowledge for the AI
                // when answering delivery timeline questions.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 4,
                    SopCode = "SOP-PROC-002",
                    Title = "Supplier Lead Time Requirements",
                    Description = "Minimum lead time requirements by material category for purchase order planning and supplier evaluation.",
                    FullText = "1. SCOPE: These lead times apply to all purchase orders issued to approved suppliers.\n\n" +
                               "2. STANDARD LEAD TIMES (calendar days from PO confirmation):\n" +
                               "   - Imported shell fabric: 45–60 days\n" +
                               "   - Local shell fabric: 14–21 days\n" +
                               "   - Lining and interlining: 14–21 days\n" +
                               "   - Buttons, rivets, snaps: 21–30 days\n" +
                               "   - Zippers: 21–30 days\n" +
                               "   - Woven labels and hang tags: 14–21 days\n" +
                               "   - Printed labels (care/content): 7–10 days\n" +
                               "   - Packaging (polybags, cartons): 7–14 days\n" +
                               "   - Thread: 7 days (maintain safety stock for standard colours)\n\n" +
                               "3. EXPEDITE POLICY: Rush orders (shorter than standard lead time) incur a 15% surcharge and require Procurement Manager approval. Supplier must confirm expedite feasibility within 48 hours of PO issuance.\n\n" +
                               "4. DELIVERY WINDOW: Suppliers have a ±3 day delivery window from the agreed date. Deliveries outside this window are recorded and count toward the supplier scorecard.\n\n" +
                               "5. FAILURE: Two consecutive late deliveries trigger a formal supplier review meeting. Three consecutive failures may result in probation or delisting.",
                    Category = "Procurement",
                    IsActive = true,
                    DisplayOrder = 4,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 5: Shipping — Export Packing Standards
                // 🎓 Defines how garments must be packed for export shipments.
                // Attached to Shipping-related report types and the
                // Commercial Invoice report context.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 5,
                    SopCode = "SOP-SHIP-001",
                    Title = "Export Packing Standards",
                    Description = "Standard packing requirements for garment export shipments including polybag, carton, and palletisation specifications.",
                    FullText = "1. SCOPE: Applies to all export shipments unless buyer-specific packing instructions override.\n\n" +
                               "2. INDIVIDUAL PACKING:\n" +
                               "   - Each garment must be folded per buyer's fold specification (or standard fold if none provided)\n" +
                               "   - Poly-bagged individually with suffocation warning printed on bag\n" +
                               "   - Size sticker on polybag matching the garment's size label\n" +
                               "   - Tissue paper for delicate fabrics (silk, satin, organza)\n\n" +
                               "3. CARTON PACKING:\n" +
                               "   - Use double-wall corrugated cartons (minimum 200 GSM burst strength)\n" +
                               "   - Maximum carton weight: 22 kg gross\n" +
                               "   - Assortment: solid colour / solid size unless buyer requests ratio pack\n" +
                               "   - Silica gel packets (2 per carton) for moisture control\n" +
                               "   - Carton dimensions must not exceed 60×45×40 cm unless buyer specifies\n\n" +
                               "4. CARTON MARKING:\n" +
                               "   - Buyer name, PO number, style number, colour, size range\n" +
                               "   - Carton number (sequential), gross/net weight, dimensions\n" +
                               "   - Country of origin, 'Made in Sri Lanka'\n" +
                               "   - Handling symbols: fragile, keep dry, this way up\n\n" +
                               "5. PALLETISATION: Standard 48×40 inch pallets, maximum 8 cartons high, stretch-wrapped with corner protectors.",
                    Category = "Shipping",
                    IsActive = true,
                    DisplayOrder = 5,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 6: Shipping — Export Documentation Checklist
                // 🎓 Lists the mandatory documents for every export shipment.
                // Linked to the Commercial Invoice report context.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 6,
                    SopCode = "SOP-SHIP-002",
                    Title = "Export Documentation Checklist",
                    Description = "Mandatory documents required for every export shipment, with responsibility assignments and submission deadlines.",
                    FullText = "1. SCOPE: Every export shipment must include the following documents. Missing or incorrect documents delay customs clearance and may incur demurrage charges.\n\n" +
                               "2. MANDATORY DOCUMENTS:\n" +
                               "   a) Commercial Invoice — issued by Finance, 3 originals + 3 copies\n" +
                               "   b) Packing List — issued by Warehouse, matching carton-level detail\n" +
                               "   c) Bill of Lading / Airway Bill — issued by freight forwarder\n" +
                               "   d) Certificate of Origin (GSP Form A or standard CO) — issued by Chamber of Commerce\n" +
                               "   e) Beneficiary Certificate — issued by Finance\n" +
                               "   f) Inspection Certificate (if buyer requires) — issued by third-party inspector\n" +
                               "   g) Fumigation Certificate (for wooden pallets) — issued by fumigation provider\n\n" +
                               "3. DEADLINES:\n" +
                               "   - All documents submitted to the bank within 5 working days of B/L date\n" +
                               "   - Discrepancies resolved within 48 hours of bank notification\n\n" +
                               "4. COPIES: Retain one complete set of copies in the export file for 7 years (statutory requirement).\n\n" +
                               "5. DIGITAL RECORDS: Scan all original documents and upload to the ERP system's shipment record within 24 hours of submission.",
                    Category = "Shipping",
                    IsActive = true,
                    DisplayOrder = 6,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 7: General — Company-Wide Confidentiality Policy
                // 🎓 A global SOP attached to "All" + "*" via SopApplicability.
                // Demonstrates the "applies everywhere" pattern and the
                // IsExcluded override (excluded from Buyer/205 in the seed).
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 7,
                    SopCode = "SOP-GEN-001",
                    Title = "Buyer Confidentiality and Data Protection Policy",
                    Description = "Company-wide policy on handling buyer proprietary information, design files, and pricing data.",
                    FullText = "1. SCOPE: This policy applies to all employees, contractors, and third-party agents who handle buyer information in any form.\n\n" +
                               "2. CONFIDENTIAL INFORMATION INCLUDES:\n" +
                               "   - Buyer-specific pricing, cost breakdowns, and margin data\n" +
                               "   - Design files, tech packs, and proprietary patterns\n" +
                               "   - Order quantities, delivery schedules, and sourcing strategies\n" +
                               "   - Buyer contact details and organisational structure\n\n" +
                               "3. HANDLING RULES:\n" +
                               "   - Never share one buyer's pricing or designs with another buyer\n" +
                               "   - Store physical tech packs in locked cabinets; return or shred after order completion\n" +
                               "   - Digital files must be stored in buyer-specific folders with role-based access\n" +
                               "   - Do not discuss buyer details in shared workspaces or common areas\n\n" +
                               "4. EMAIL / DIGITAL TRANSMISSION:\n" +
                               "   - Verify recipient addresses before sending confidential attachments\n" +
                               "   - Use password-protected ZIP files for tech packs sent externally\n" +
                               "   - Never use personal email accounts for buyer correspondence\n\n" +
                               "5. BREACH: Any suspected breach must be reported to the Compliance Officer within 4 hours. Confirmed breaches may result in disciplinary action up to and including termination.",
                    Category = "General",
                    IsActive = true,
                    DisplayOrder = 7,
                    EffectiveFrom = new DateTime(2025, 1, 1),
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                },

                // ─────────────────────────────────────────────────────────
                // SOP 8: Quality — Pre-Production Sample Approval (DRAFT)
                // 🎓 An INACTIVE SOP to demonstrate the IsActive = false
                // state in the admin UI. Shows how SOPs can be drafted
                // and kept hidden until formally approved.
                // ─────────────────────────────────────────────────────────
                new StandardOperatingProcedure
                {
                    SopId = 8,
                    SopCode = "SOP-QC-003",
                    Title = "Pre-Production Sample Approval Process (DRAFT)",
                    Description = "Draft procedure for managing pre-production sample submissions, buyer approvals, and revision tracking.",
                    FullText = "1. SCOPE: Covers all pre-production (PP) sample submissions from initial sample through to final buyer approval.\n\n" +
                               "2. SAMPLE TYPES (in sequence):\n" +
                               "   a) Development Sample — initial interpretation of buyer's design\n" +
                               "   b) Fit Sample — correct measurements per graded spec\n" +
                               "   c) Pre-Production Sample — final materials, trims, and construction\n" +
                               "   d) Size Set Sample — full size range for grading verification\n" +
                               "   e) Top of Production (TOP) — first off the line for final sign-off\n\n" +
                               "3. SUBMISSION: Each sample must include a Sample Submission Form with style reference, date, revision number, and changes from previous submission.\n\n" +
                               "4. APPROVAL TRACKING: Record buyer's response (Approved / Approved with Comments / Rejected) in the ERP sample tracker within 24 hours of receipt.\n\n" +
                               "5. MAXIMUM REVISIONS: If a sample is rejected 3 times, escalate to the Merchandising Manager for a buyer meeting before the 4th submission.\n\n" +
                               "6. TIMELINE: Standard turnaround for each sample stage is 5 working days from approval of the previous stage.",
                    Category = "Quality",
                    IsActive = false,    // 🎓 DRAFT — not yet active, won't appear on reports or in RAG
                    DisplayOrder = 8,
                    EffectiveFrom = new DateTime(2025, 7, 1),  // 🎓 Future effective date
                    EffectiveTo = null,
                    CreatedBy = "SYSTEM-SEED",
                    CreatedAt = new DateTime(2025, 1, 1),
                    ModifiedBy = "SYSTEM-SEED",
                    ModifiedAt = new DateTime(2025, 1, 1)
                }
            );
        }
    }
}
