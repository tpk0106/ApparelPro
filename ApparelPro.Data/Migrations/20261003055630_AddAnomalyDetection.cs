using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnomalyDetection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnomalyAlerts",
                columns: table => new
                {
                    AlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    AnomalyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpectedValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DeviationPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecommendedAction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NEW"),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnomalyAlerts", x => x.AlertId);
                });

            migrationBuilder.CreateTable(
                name: "AnomalyRules",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnomalyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LowThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 10.0m),
                    MediumThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 25.0m),
                    HighThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 50.0m),
                    CriticalThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 100.0m),
                    BuyerCode = table.Column<int>(type: "int", nullable: true),
                    StockCodeFilter = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnomalyRules", x => x.RuleId);
                });

            migrationBuilder.InsertData(
                table: "AnomalyRules",
                columns: new[] { "RuleId", "AnomalyType", "BuyerCode", "CreatedAt", "CriticalThreshold", "Description", "HighThreshold", "IsActive", "LowThreshold", "MediumThreshold", "RuleName", "SortOrder", "StockCodeFilter", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "OVER_CONSUMPTION", null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 100.0m, "Flags when actual issued quantity exceeds planned consumption plus percentage allowance. Compares OrderwiseStockMaster.IssuedQuantity against StyleMaterialCostProfile.TotalConsumption adjusted by StyleMaterialConsumptionLedger.PercentageAllowance.", 50.0m, true, 10.0m, 25.0m, "Default Over-Consumption Detection", 1, null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "PRICE_SPIKE", null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 120.0m, "Flags when a material's current unit price in StyleMaterialCostProfile significantly exceeds the historical average price for the same ItemCode across all styles. Uses rolling average of UnitPrice grouped by ItemCode.", 60.0m, true, 15.0m, 30.0m, "Default Price Spike Detection", 2, null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "WASTE_DAMAGE", null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 40.0m, "Flags when damaged or supplier-returned quantities in OrderwiseStockMaster exceed a percentage of the total ordered quantity. DamagedQuantity + SupplierReturnQuantity compared against OrderedQuantity.", 20.0m, true, 5.0m, 10.0m, "Default Waste & Damage Detection", 3, null, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_Fingerprint_Status",
                table: "AnomalyAlerts",
                columns: new[] { "Fingerprint", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_Status_Detected",
                table: "AnomalyAlerts",
                columns: new[] { "Status", "DetectedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_Style",
                table: "AnomalyAlerts",
                columns: new[] { "BuyerCode", "Order", "TypeCode", "StyleCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_Type",
                table: "AnomalyAlerts",
                column: "AnomalyType");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyRules_Type_Active",
                table: "AnomalyRules",
                columns: new[] { "AnomalyType", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnomalyAlerts");

            migrationBuilder.DropTable(
                name: "AnomalyRules");
        }
    }
}
