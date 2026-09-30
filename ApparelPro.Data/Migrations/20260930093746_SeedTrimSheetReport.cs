using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTrimSheetReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReportRegistries",
                columns: new[] { "ReportCode", "Category", "CommonNames", "Description", "DisplayName", "DisplayOrder", "EndpointTemplate", "IsActive", "ParamSources", "RequiredParams" },
                values: new object[] { "TrimSheet", "OrderManagement", "trim sheet, material consumption, material consumption report, BOM report, bill of materials, costing sheet, trim costing, material cost breakdown, fabric and trim breakdown, garment costing", "Material consumption and costing breakdown for a style — shows all trims, fabrics, and accessories with quantities, unit prices, supplier assignments, stock group subtotals, and estimated profit margin.", "Trim Sheet Report", 1, "api/trim-sheet-report/pdf", true, "{\"buyerCode\":{\"entity\":\"Style\",\"field\":\"BuyerCode\",\"type\":\"int\",\"description\":\"Buyer code from the Style entity\"},\"order\":{\"entity\":\"Style\",\"field\":\"Order\",\"type\":\"string\",\"description\":\"Purchase order number from the Style entity\"},\"typeCode\":{\"entity\":\"Style\",\"field\":\"TypeCode\",\"type\":\"int\",\"description\":\"Garment type code from the Style entity\"},\"styleCode\":{\"entity\":\"Style\",\"field\":\"StyleCode\",\"type\":\"string\",\"description\":\"Style code — the primary identifier the user mentions\"}}", "[\"buyerCode\",\"order\",\"typeCode\",\"styleCode\"]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReportRegistries",
                keyColumn: "ReportCode",
                keyValue: "TrimSheet");
        }
    }
}
