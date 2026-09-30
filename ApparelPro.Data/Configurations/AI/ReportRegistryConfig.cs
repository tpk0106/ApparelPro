// ═══════════════════════════════════════════════════════════════════════════
//  ReportRegistryConfig.cs — EF Core Fluent API Configuration
//  Location: ApparelPro.Data/Configurations/AI/ReportRegistryConfig.cs
// ═══════════════════════════════════════════════════════════════════════════
//
// 🎓 WHY FLUENT API INSTEAD OF DATA ANNOTATIONS?
// ApparelPro uses Fluent API exclusively for ALL entity configurations.
// This keeps the entity classes clean (pure POCOs) and centralises all
// database mapping in one place per entity.
//
// 🎓 NAMING CONVENTION:
// Config classes follow the pattern: {Entity}Config
// (NOT {Entity}Configuration — that's a different project convention)
// Namespace: ApparelPro.Data.Configurations.{DomainFolder}
//
// 🎓 TABLE NAME:
// Per the project's pluralisation rule, the entity "ReportRegistry"
// maps to the table "ReportRegistries" (plural).
// ═══════════════════════════════════════════════════════════════════════════

using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI
{
    public class ReportRegistryConfig : IEntityTypeConfiguration<ReportRegistry>
    {
        public void Configure(EntityTypeBuilder<ReportRegistry> entity)
        {
            // ── Table Mapping ────────────────────────────────────────────
            // 🎓 Explicit table name ensures EF Core doesn't rely on DbSet
            // property naming conventions. Plural form per project rule.
            entity.ToTable("ReportRegistries");

            // ── Primary Key ──────────────────────────────────────────────
            // 🎓 String PK — meaningful business identifier like "TrimSheet".
            // NOT auto-increment because report codes are stable, human-readable
            // identifiers that appear in logs, prompts, and seed scripts.
            entity.HasKey(k => k.ReportCode);

            // ── Column Configurations ────────────────────────────────────
            // 🎓 Every property gets explicit HasColumnType() and HasMaxLength()
            // per the project convention (see BuyerConfig.cs, StyleConfig.cs).

            entity.Property(p => p.ReportCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar");

            entity.Property(p => p.DisplayName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar");

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar");

            entity.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar");

            // 🎓 RequiredParams: JSON array stored as nvarchar(max).
            // Example: ["buyerCode","order","typeCode","styleCode"]
            // We use nvarchar(max) because the JSON could grow as reports
            // gain more parameters. Default is "[]" (empty JSON array).
            entity.Property(p => p.RequiredParams)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            // 🎓 ParamSources: Optional JSON object describing parameter resolution.
            // Can be null for reports that don't need parameter extraction from RAG.
            entity.Property(p => p.ParamSources)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            entity.Property(p => p.EndpointTemplate)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("nvarchar");

            // 🎓 CommonNames: The synonym context field — comma-separated aliases.
            // Optional because some reports might be known by only one name.
            // nvarchar(max) because the alias list can grow as users discover
            // new phrases that should be recognised.
            entity.Property(p => p.CommonNames)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            entity.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnType("bit");

            entity.Property(p => p.DisplayOrder)
                .IsRequired()
                .HasColumnType("int");

            // ── Indexes ──────────────────────────────────────────────────
            // 🎓 Index on Category for filtered lookups ("show me all OrderManagement reports").
            // Index on IsActive for the common query pattern of "get all active reports".
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.IsActive);

            // ── Seed Data ────────────────────────────────────────────────
            // 🎓 HasData() tells EF Core to include this row in the migration.
            // When you run "dotnet ef migrations add AddReportRegistry", EF will
            // generate an InsertData() call in the migration's Up() method.
            //
            // 🎓 WHY SEED HERE INSTEAD OF A SEPARATE SQL SCRIPT?
            // Seeding via HasData() keeps everything in one place — the entity,
            // its configuration, AND its initial data are all managed by EF Core.
            // The migration is the single source of truth for schema + seed.
            //
            // 🎓 IMPORTANT: HasData() is for STATIC seed data only.
            // If you later change these values, EF will detect the diff and
            // generate an UpdateData() call in the next migration.
            entity.HasData(
                new ReportRegistry
                {
                    ReportCode = "TrimSheet",
                    DisplayName = "Trim Sheet Report",

                    // 🎓 Description helps Claude disambiguate "consumption" in different contexts
                    Description = "Material consumption and costing breakdown for a style — shows all trims, fabrics, and accessories with quantities, unit prices, supplier assignments, stock group subtotals, and estimated profit margin.",

                    Category = "OrderManagement",

                    // 🎓 RequiredParams — JSON array of parameter names the endpoint needs
                    RequiredParams = "[\"buyerCode\",\"order\",\"typeCode\",\"styleCode\"]",

                    // 🎓 ParamSources — tells the backend how to extract each param
                    // from the matched Style entity in the RAG chunks
                    ParamSources = "{\"buyerCode\":{\"entity\":\"Style\",\"field\":\"BuyerCode\",\"type\":\"int\",\"description\":\"Buyer code from the Style entity\"},\"order\":{\"entity\":\"Style\",\"field\":\"Order\",\"type\":\"string\",\"description\":\"Purchase order number from the Style entity\"},\"typeCode\":{\"entity\":\"Style\",\"field\":\"TypeCode\",\"type\":\"int\",\"description\":\"Garment type code from the Style entity\"},\"styleCode\":{\"entity\":\"Style\",\"field\":\"StyleCode\",\"type\":\"string\",\"description\":\"Style code — the primary identifier the user mentions\"}}",

                    EndpointTemplate = "api/trim-sheet-report/pdf",

                    // 🎓 CommonNames — injected into Claude's system prompt for NLU context.
                    // Claude's own language understanding does the fuzzy matching from there.
                    CommonNames = "trim sheet, material consumption, material consumption report, BOM report, bill of materials, costing sheet, trim costing, material cost breakdown, fabric and trim breakdown, garment costing",

                    IsActive = true,
                    DisplayOrder = 1
                }
            );
        }
    }
}
