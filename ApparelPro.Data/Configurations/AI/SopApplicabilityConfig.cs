// ═══════════════════════════════════════════════════════════════════════════
//  SopApplicabilityConfig.cs — EF Core Fluent API Configuration
//  Location: ApparelPro.Data/Configurations/AI/SopApplicabilityConfig.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHY A SEPARATE CONFIG FILE?
// Each entity gets its own Config class in ApparelPro. This keeps the
// Fluent API configuration modular and follows the same pattern as
// ReportRegistryConfig.cs, AiChatSessionConfiguration.cs, etc.
//
// 🎓 SEED DATA STRATEGY:
// The SopApplicability seeds LINK the parent SOPs (seeded in
// StandardOperatingProcedureConfig.cs) to specific contexts:
//   - SOP 1 (Fabric Inspection) → TrimSheet reports + all buyers
//   - SOP 2 (Measurement Tolerance) → all reports, all entity types
//   - SOP 3 (Wastage Allowance) → TrimSheet reports specifically
//   - SOP 4 (Lead Times) → Supplier-level attachment
//   - SOP 5 (Packing) → Shipping context
//   - SOP 6 (Export Docs) → Shipping context
//   - SOP 7 (Confidentiality) → All/* global with an exclusion override
//   - SOP 8 (DRAFT, inactive) → TrimSheet but won't appear since IsActive=false
//
// 🎓 IMPORTANT — FK DEPENDENCY:
// SopApplicability.SopId references StandardOperatingProcedure.SopId.
// EF Core handles seed ordering automatically — it will insert the parent
// SOP rows before the child applicability rows in the generated migration.
// ═══════════════════════════════════════════════════════════════════════════

using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI
{
    public class SopApplicabilityConfig : IEntityTypeConfiguration<SopApplicability>
    {
        public void Configure(EntityTypeBuilder<SopApplicability> entity)
        {
            // ── Table Mapping ────────────────────────────────────────────
            // 🎓 Plural form per project's mandatory pluralisation rule.
            entity.ToTable("SopApplicabilities");

            // ── Primary Key ──────────────────────────────────────────────
            // 🎓 Auto-increment int PK — simple surrogate key for the
            // linking table. The business meaning comes from the composite
            // of SopId + ApplicabilityType + ApplicabilityKey.
            entity.HasKey(k => k.SopApplicabilityId);

            entity.Property(p => p.SopApplicabilityId)
                .UseIdentityColumn();

            // ── Column Configurations ────────────────────────────────────

            // 🎓 SopId: FK to StandardOperatingProcedure. Required.
            // The relationship itself is configured in StandardOperatingProcedureConfig.cs
            // (HasMany → WithOne → HasForeignKey), but the column property
            // still needs explicit configuration here.
            entity.Property(p => p.SopId)
                .IsRequired()
                .HasColumnType("int");

            // 🎓 ApplicabilityType: The KIND of context this rule targets.
            // Valid values: "ReportType", "EntityType", "Supplier", "Buyer", "All"
            // Max 50 chars is generous — the longest current value is "EntityType" (10 chars).
            entity.Property(p => p.ApplicabilityType)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar");

            // 🎓 ApplicabilityKey: The SPECIFIC instance within that type.
            // For "ReportType" → "TrimSheet", for "Buyer" → "101", for "All" → "*"
            // Max 100 chars handles any reasonable entity code or identifier.
            entity.Property(p => p.ApplicabilityKey)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar");

            // 🎓 IsExcluded: Defaults to false (inclusion rule).
            // When true, this rule EXCLUDES the SOP from the target context.
            entity.Property(p => p.IsExcluded)
                .IsRequired()
                .HasColumnType("bit")
                .HasDefaultValue(false);

            // ── Indexes ──────────────────────────────────────────────────

            // 🎓 Composite index on {SopId, ApplicabilityType, ApplicabilityKey}
            // This is the PRIMARY lookup path — when the PDF engine asks
            // "which SOPs apply to TrimSheet reports?", it queries:
            //   WHERE ApplicabilityType = 'ReportType' AND ApplicabilityKey = 'TrimSheet'
            // Including SopId in the index lets EF efficiently join back to the parent.
            entity.HasIndex(p => new { p.SopId, p.ApplicabilityType, p.ApplicabilityKey });

            // 🎓 Index on ApplicabilityType alone — for admin UI filters
            // ("show me all rules of type Buyer").
            entity.HasIndex(p => p.ApplicabilityType);

            // ── Seed Data ────────────────────────────────────────────────
            // 🎓 These rows connect the 8 seeded SOPs from
            // StandardOperatingProcedureConfig.cs to their target contexts.
            //
            // 🎓 SEED PK VALUES:
            // SopApplicabilityId starts at 1 and increments. If you add more
            // seeds later, continue from the next available ID.
            //
            // 🎓 DEMONSTRATES THESE PATTERNS:
            //   1. ReportType attachment (SOP on a specific report)
            //   2. Buyer attachment (SOP for a specific buyer)
            //   3. Supplier attachment (SOP for a specific supplier)
            //   4. All/* global attachment (SOP everywhere)
            //   5. IsExcluded = true override (exclude from one buyer)
            //   6. EntityType attachment (SOP for a business entity type)

            entity.HasData(

                // ─────────────────────────────────────────────────────────
                // SOP 1 (Fabric Inspection) Applicability Rules
                // 🎓 Appears on TrimSheet reports AND applies to all buyers.
                // Two separate rules — one per context dimension.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 1,
                    SopId = 1,
                    ApplicabilityType = "ReportType",
                    ApplicabilityKey = "TrimSheet",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 2,
                    SopId = 1,
                    ApplicabilityType = "All",
                    ApplicabilityKey = "*",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 2 (Measurement Tolerance) Applicability Rules
                // 🎓 Global quality standard — applies to all report types
                // and entity types. Two rules for broad coverage.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 3,
                    SopId = 2,
                    ApplicabilityType = "All",
                    ApplicabilityKey = "*",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 4,
                    SopId = 2,
                    ApplicabilityType = "EntityType",
                    ApplicabilityKey = "Style",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 3 (Wastage Allowance) Applicability Rules
                // 🎓 Specifically for TrimSheet reports — wastage factors
                // are directly relevant to material consumption costing.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 5,
                    SopId = 3,
                    ApplicabilityType = "ReportType",
                    ApplicabilityKey = "TrimSheet",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 4 (Supplier Lead Times) Applicability Rules
                // 🎓 Attached to a specific supplier code to demonstrate
                // the Supplier attachment pattern. Also attached to the
                // EntityType "PurchaseOrder" since lead times are most
                // relevant in the PO context.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 6,
                    SopId = 4,
                    ApplicabilityType = "Supplier",
                    ApplicabilityKey = "FABRIC-001",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 7,
                    SopId = 4,
                    ApplicabilityType = "EntityType",
                    ApplicabilityKey = "PurchaseOrder",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 5 (Export Packing) Applicability Rules
                // 🎓 Attached to the Shipping entity type context.
                // Also attached to a specific buyer code to demonstrate
                // buyer-level SOP attachment.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 8,
                    SopId = 5,
                    ApplicabilityType = "EntityType",
                    ApplicabilityKey = "Shipment",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 9,
                    SopId = 5,
                    ApplicabilityType = "Buyer",
                    ApplicabilityKey = "101",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 6 (Export Documentation) Applicability Rules
                // 🎓 Same shipping context as SOP 5. Also linked to
                // Buyer/101 for the commercial invoice workflow.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 10,
                    SopId = 6,
                    ApplicabilityType = "EntityType",
                    ApplicabilityKey = "Shipment",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 11,
                    SopId = 6,
                    ApplicabilityType = "Buyer",
                    ApplicabilityKey = "101",
                    IsExcluded = false
                },

                // ─────────────────────────────────────────────────────────
                // SOP 7 (Confidentiality) Applicability Rules
                // 🎓 GLOBAL scope (All/*) — this SOP appears on every
                // report for every buyer, EXCEPT buyer 205.
                //
                // 🎓 DEMONSTRATES THE IsExcluded OVERRIDE PATTERN:
                // Row 12: Include everywhere (All/*)
                // Row 13: EXCLUDE from buyer 205 (Buyer/205, IsExcluded=true)
                // Result: Every buyer except 205 sees this SOP.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 12,
                    SopId = 7,
                    ApplicabilityType = "All",
                    ApplicabilityKey = "*",
                    IsExcluded = false
                },
                new SopApplicability
                {
                    SopApplicabilityId = 13,
                    SopId = 7,
                    ApplicabilityType = "Buyer",
                    ApplicabilityKey = "205",
                    IsExcluded = true   // 🎓 EXCLUSION — buyer 205 opted out of this policy
                },

                // ─────────────────────────────────────────────────────────
                // SOP 8 (DRAFT — Pre-Production Samples) Applicability
                // 🎓 Even though this has an applicability rule, the SOP
                // itself has IsActive = false, so it WON'T appear on any
                // reports or in RAG results until activated.
                // ─────────────────────────────────────────────────────────
                new SopApplicability
                {
                    SopApplicabilityId = 14,
                    SopId = 8,
                    ApplicabilityType = "ReportType",
                    ApplicabilityKey = "TrimSheet",
                    IsExcluded = false
                }
            );
        }
    }
}
