using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionLineAllocationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstimatedProductionLineAllocations",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    EstimatedProductionPerDay = table.Column<decimal>(type: "decimal(9,0)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    LeadTimeDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ShipDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    NumberOfDays = table.Column<decimal>(type: "decimal(6,1)", nullable: false),
                    EstimatedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimatedProductionLineAllocations", x => new { x.BuyerCode, x.StyleCode });
                    table.ForeignKey(
                        name: "FK_EstimatedProductionLineAllocations_ProductionLines_LineCode",
                        column: x => x.LineCode,
                        principalTable: "ProductionLines",
                        principalColumn: "LineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLineAllocations",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ShipmentOrder = table.Column<string>(type: "varchar(12)", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    EstimatedProductionPerDay = table.Column<decimal>(type: "decimal(9,0)", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    LeadTimeDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    NumberOfMachines = table.Column<int>(type: "int", nullable: false),
                    CostPerDay = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    NumberOfDays = table.Column<decimal>(type: "decimal(6,1)", nullable: false),
                    OriginalEstimatedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OriginalEstimatedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLineAllocations", x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.ShipmentOrder, x.LineCode });
                    table.ForeignKey(
                        name: "FK_ProductionLineAllocations_Currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLineAllocations_ProductionLines_LineCode",
                        column: x => x.LineCode,
                        principalTable: "ProductionLines",
                        principalColumn: "LineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstimatedProductionLineAllocations_LineCode",
                table: "EstimatedProductionLineAllocations",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLineAllocations_CurrencyCode",
                table: "ProductionLineAllocations",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLineAllocations_LineCode",
                table: "ProductionLineAllocations",
                column: "LineCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstimatedProductionLineAllocations");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "ProductionLineAllocations");
        }
    }
}
