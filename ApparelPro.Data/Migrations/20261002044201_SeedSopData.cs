using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSopData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardOperatingProcedures_IsActive_EffectiveFrom",
                table: "StandardOperatingProcedures");

            migrationBuilder.DropIndex(
                name: "IX_SopApplicabilities_ApplicabilityType_ApplicabilityKey",
                table: "SopApplicabilities");

            migrationBuilder.DropIndex(
                name: "IX_SopApplicabilities_SopId_ApplicabilityType_ApplicabilityKey_IsExcluded",
                table: "SopApplicabilities");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "StandardOperatingProcedures",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveTo",
                table: "StandardOperatingProcedures",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveFrom",
                table: "StandardOperatingProcedures",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<bool>(
                name: "IsExcluded",
                table: "SopApplicabilities",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.InsertData(
                table: "StandardOperatingProcedures",
                columns: new[] { "SopId", "Category", "CreatedAt", "CreatedBy", "Description", "DisplayOrder", "EffectiveFrom", "EffectiveTo", "FullText", "IsActive", "ModifiedAt", "ModifiedBy", "SopCode", "Title" },
                values: new object[,]
                {
                    { 1, "Quality", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Standard inspection procedure for incoming fabric rolls using the four-point grading system at AQL 2.5 acceptance level.", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: This procedure applies to all incoming fabric rolls received at the warehouse before cutting.\n\n2. SAMPLING: Inspect a minimum of 10% of total rolls per delivery, selected randomly across all colour lots. For orders under 500 yards, inspect 100% of rolls.\n\n3. FOUR-POINT GRADING:\n   - Defects up to 3 inches: 1 point\n   - Defects 3–6 inches: 2 points\n   - Defects 6–9 inches: 3 points\n   - Defects over 9 inches: 4 points\n\n4. ACCEPTANCE: Total points per 100 linear yards must not exceed 40 points. Rolls exceeding this threshold are flagged for return or downgrading.\n\n5. DOCUMENTATION: Record all inspection results in the Fabric Inspection Log with roll number, colour lot, defect count, and total penalty points. Attach photographs of major defects.\n\n6. ESCALATION: If more than 20% of inspected rolls fail, notify the Procurement Manager and issue a Supplier Non-Conformance Report (NCR) within 24 hours.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-QC-001", "Fabric Inspection — AQL 2.5 Four-Point System" },
                    { 2, "Quality", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Acceptable measurement deviation limits at cutting, sewing, and finishing stages for all garment types.", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: Applicable to all garment types across cutting, sewing, and finishing stages.\n\n2. CUTTING TOLERANCE:\n   - Critical measurements (chest, waist, hip): ±¼ inch (±6mm)\n   - Non-critical measurements (sleeve opening, hem width): ±⅜ inch (±10mm)\n\n3. SEWING TOLERANCE:\n   - Seam allowance: ±1/16 inch (±2mm) from spec\n   - Stitch count: 10–12 stitches per inch unless buyer spec states otherwise\n   - Critical points (collar, cuff): ±⅛ inch (±3mm)\n\n4. FINISHING TOLERANCE:\n   - After wash/press: overall ±½ inch (±13mm) from buyer spec\n   - Shrinkage allowance must be pre-calculated per fabric test report\n\n5. AUDIT: Measure 5 garments per size per colour lot. All 5 must pass for the lot to proceed. If any single garment fails by more than double the tolerance, quarantine the entire lot.\n\n6. RECORDS: File measurement audit sheets with the production batch record. Retain for 12 months post-shipment.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-QC-002", "Garment Measurement Tolerance — Production Stages" },
                    { 3, "Procurement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Maximum allowable wastage percentages by material type for consumption and costing calculations.", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: This policy governs the wastage factor applied when calculating material requirements for purchase orders and costing sheets.\n\n2. STANDARD WASTAGE ALLOWANCES:\n   - Shell fabric (woven): 5% of net consumption\n   - Shell fabric (knit): 8% of net consumption\n   - Lining fabric: 3% of net consumption\n   - Interlining / Fusible: 5% of net consumption\n   - Trims (buttons, zippers, labels): 2% of net consumption\n   - Thread: 10% of net consumption\n   - Elastic / Drawcord: 3% of net consumption\n\n3. OVERRIDES: Buyer-specific wastage factors take precedence if explicitly agreed in the order contract. Document any override in the style master's notes field.\n\n4. CALCULATION: Gross requirement = Net consumption × (1 + wastage%). This gross figure is what appears on purchase orders and supplier RFQs.\n\n5. REVIEW: Wastage allowances are reviewed quarterly against actual production waste data. If actual waste exceeds the allowance by more than 1% for two consecutive quarters, escalate to the Production Manager for root cause analysis.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-PROC-001", "Material Wastage Allowance Policy" },
                    { 4, "Procurement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Minimum lead time requirements by material category for purchase order planning and supplier evaluation.", 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: These lead times apply to all purchase orders issued to approved suppliers.\n\n2. STANDARD LEAD TIMES (calendar days from PO confirmation):\n   - Imported shell fabric: 45–60 days\n   - Local shell fabric: 14–21 days\n   - Lining and interlining: 14–21 days\n   - Buttons, rivets, snaps: 21–30 days\n   - Zippers: 21–30 days\n   - Woven labels and hang tags: 14–21 days\n   - Printed labels (care/content): 7–10 days\n   - Packaging (polybags, cartons): 7–14 days\n   - Thread: 7 days (maintain safety stock for standard colours)\n\n3. EXPEDITE POLICY: Rush orders (shorter than standard lead time) incur a 15% surcharge and require Procurement Manager approval. Supplier must confirm expedite feasibility within 48 hours of PO issuance.\n\n4. DELIVERY WINDOW: Suppliers have a ±3 day delivery window from the agreed date. Deliveries outside this window are recorded and count toward the supplier scorecard.\n\n5. FAILURE: Two consecutive late deliveries trigger a formal supplier review meeting. Three consecutive failures may result in probation or delisting.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-PROC-002", "Supplier Lead Time Requirements" },
                    { 5, "Shipping", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Standard packing requirements for garment export shipments including polybag, carton, and palletisation specifications.", 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: Applies to all export shipments unless buyer-specific packing instructions override.\n\n2. INDIVIDUAL PACKING:\n   - Each garment must be folded per buyer's fold specification (or standard fold if none provided)\n   - Poly-bagged individually with suffocation warning printed on bag\n   - Size sticker on polybag matching the garment's size label\n   - Tissue paper for delicate fabrics (silk, satin, organza)\n\n3. CARTON PACKING:\n   - Use double-wall corrugated cartons (minimum 200 GSM burst strength)\n   - Maximum carton weight: 22 kg gross\n   - Assortment: solid colour / solid size unless buyer requests ratio pack\n   - Silica gel packets (2 per carton) for moisture control\n   - Carton dimensions must not exceed 60×45×40 cm unless buyer specifies\n\n4. CARTON MARKING:\n   - Buyer name, PO number, style number, colour, size range\n   - Carton number (sequential), gross/net weight, dimensions\n   - Country of origin, 'Made in Sri Lanka'\n   - Handling symbols: fragile, keep dry, this way up\n\n5. PALLETISATION: Standard 48×40 inch pallets, maximum 8 cartons high, stretch-wrapped with corner protectors.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-SHIP-001", "Export Packing Standards" },
                    { 6, "Shipping", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Mandatory documents required for every export shipment, with responsibility assignments and submission deadlines.", 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: Every export shipment must include the following documents. Missing or incorrect documents delay customs clearance and may incur demurrage charges.\n\n2. MANDATORY DOCUMENTS:\n   a) Commercial Invoice — issued by Finance, 3 originals + 3 copies\n   b) Packing List — issued by Warehouse, matching carton-level detail\n   c) Bill of Lading / Airway Bill — issued by freight forwarder\n   d) Certificate of Origin (GSP Form A or standard CO) — issued by Chamber of Commerce\n   e) Beneficiary Certificate — issued by Finance\n   f) Inspection Certificate (if buyer requires) — issued by third-party inspector\n   g) Fumigation Certificate (for wooden pallets) — issued by fumigation provider\n\n3. DEADLINES:\n   - All documents submitted to the bank within 5 working days of B/L date\n   - Discrepancies resolved within 48 hours of bank notification\n\n4. COPIES: Retain one complete set of copies in the export file for 7 years (statutory requirement).\n\n5. DIGITAL RECORDS: Scan all original documents and upload to the ERP system's shipment record within 24 hours of submission.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-SHIP-002", "Export Documentation Checklist" },
                    { 7, "General", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Company-wide policy on handling buyer proprietary information, design files, and pricing data.", 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: This policy applies to all employees, contractors, and third-party agents who handle buyer information in any form.\n\n2. CONFIDENTIAL INFORMATION INCLUDES:\n   - Buyer-specific pricing, cost breakdowns, and margin data\n   - Design files, tech packs, and proprietary patterns\n   - Order quantities, delivery schedules, and sourcing strategies\n   - Buyer contact details and organisational structure\n\n3. HANDLING RULES:\n   - Never share one buyer's pricing or designs with another buyer\n   - Store physical tech packs in locked cabinets; return or shred after order completion\n   - Digital files must be stored in buyer-specific folders with role-based access\n   - Do not discuss buyer details in shared workspaces or common areas\n\n4. EMAIL / DIGITAL TRANSMISSION:\n   - Verify recipient addresses before sending confidential attachments\n   - Use password-protected ZIP files for tech packs sent externally\n   - Never use personal email accounts for buyer correspondence\n\n5. BREACH: Any suspected breach must be reported to the Compliance Officer within 4 hours. Confirmed breaches may result in disciplinary action up to and including termination.", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-GEN-001", "Buyer Confidentiality and Data Protection Policy" },
                    { 8, "Quality", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "Draft procedure for managing pre-production sample submissions, buyer approvals, and revision tracking.", 8, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "1. SCOPE: Covers all pre-production (PP) sample submissions from initial sample through to final buyer approval.\n\n2. SAMPLE TYPES (in sequence):\n   a) Development Sample — initial interpretation of buyer's design\n   b) Fit Sample — correct measurements per graded spec\n   c) Pre-Production Sample — final materials, trims, and construction\n   d) Size Set Sample — full size range for grading verification\n   e) Top of Production (TOP) — first off the line for final sign-off\n\n3. SUBMISSION: Each sample must include a Sample Submission Form with style reference, date, revision number, and changes from previous submission.\n\n4. APPROVAL TRACKING: Record buyer's response (Approved / Approved with Comments / Rejected) in the ERP sample tracker within 24 hours of receipt.\n\n5. MAXIMUM REVISIONS: If a sample is rejected 3 times, escalate to the Merchandising Manager for a buyer meeting before the 4th submission.\n\n6. TIMELINE: Standard turnaround for each sample stage is 5 working days from approval of the previous stage.", false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SYSTEM-SEED", "SOP-QC-003", "Pre-Production Sample Approval Process (DRAFT)" }
                });

            migrationBuilder.InsertData(
                table: "SopApplicabilities",
                columns: new[] { "SopApplicabilityId", "ApplicabilityKey", "ApplicabilityType", "SopId" },
                values: new object[,]
                {
                    { 1, "TrimSheet", "ReportType", 1 },
                    { 2, "*", "All", 1 },
                    { 3, "*", "All", 2 },
                    { 4, "Style", "EntityType", 2 },
                    { 5, "TrimSheet", "ReportType", 3 },
                    { 6, "FABRIC-001", "Supplier", 4 },
                    { 7, "PurchaseOrder", "EntityType", 4 },
                    { 8, "Shipment", "EntityType", 5 },
                    { 9, "101", "Buyer", 5 },
                    { 10, "Shipment", "EntityType", 6 },
                    { 11, "101", "Buyer", 6 },
                    { 12, "*", "All", 7 }
                });

            migrationBuilder.InsertData(
                table: "SopApplicabilities",
                columns: new[] { "SopApplicabilityId", "ApplicabilityKey", "ApplicabilityType", "IsExcluded", "SopId" },
                values: new object[] { 13, "205", "Buyer", true, 7 });

            migrationBuilder.InsertData(
                table: "SopApplicabilities",
                columns: new[] { "SopApplicabilityId", "ApplicabilityKey", "ApplicabilityType", "SopId" },
                values: new object[] { 14, "TrimSheet", "ReportType", 8 });

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_EffectiveFrom_EffectiveTo",
                table: "StandardOperatingProcedures",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_ApplicabilityType",
                table: "SopApplicabilities",
                column: "ApplicabilityType");

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_SopId_ApplicabilityType_ApplicabilityKey",
                table: "SopApplicabilities",
                columns: new[] { "SopId", "ApplicabilityType", "ApplicabilityKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StandardOperatingProcedures_EffectiveFrom_EffectiveTo",
                table: "StandardOperatingProcedures");

            migrationBuilder.DropIndex(
                name: "IX_SopApplicabilities_ApplicabilityType",
                table: "SopApplicabilities");

            migrationBuilder.DropIndex(
                name: "IX_SopApplicabilities_SopId_ApplicabilityType_ApplicabilityKey",
                table: "SopApplicabilities");

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SopApplicabilities",
                keyColumn: "SopApplicabilityId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "StandardOperatingProcedures",
                keyColumn: "SopId",
                keyValue: 8);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "StandardOperatingProcedures",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveTo",
                table: "StandardOperatingProcedures",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveFrom",
                table: "StandardOperatingProcedures",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsExcluded",
                table: "SopApplicabilities",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_IsActive_EffectiveFrom",
                table: "StandardOperatingProcedures",
                columns: new[] { "IsActive", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_ApplicabilityType_ApplicabilityKey",
                table: "SopApplicabilities",
                columns: new[] { "ApplicabilityType", "ApplicabilityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_SopId_ApplicabilityType_ApplicabilityKey_IsExcluded",
                table: "SopApplicabilities",
                columns: new[] { "SopId", "ApplicabilityType", "ApplicabilityKey", "IsExcluded" },
                unique: true);
        }
    }
}
