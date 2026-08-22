using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardPinnedStyleParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "ParameterKey", "Category", "DataType", "Description", "Options", "Value" },
                values: new object[,]
                {
                    { "DashboardPinnedBuyerCode", "Dashboard", "Text", "Fallback dashboard style - Buyer code. Only used when no Actual Production Entry or Daily Production Time Ticket rows exist yet.", null, "" },
                    { "DashboardPinnedOrder", "Dashboard", "Text", "Fallback dashboard style - Order number.", null, "" },
                    { "DashboardPinnedStyleCode", "Dashboard", "Text", "Fallback dashboard style - Style code.", null, "" },
                    { "DashboardPinnedTypeCode", "Dashboard", "Text", "Fallback dashboard style - Garment type code.", null, "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "DashboardPinnedBuyerCode");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "DashboardPinnedOrder");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "DashboardPinnedStyleCode");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "DashboardPinnedTypeCode");
        }
    }
}
