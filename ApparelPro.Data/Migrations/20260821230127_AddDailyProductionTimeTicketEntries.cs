using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyProductionTimeTicketEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyProductionTimeTicketEntries",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    NonProductiveHourCode = table.Column<string>(type: "nvarchar(2)", nullable: true),
                    NonProductiveHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    WorkHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProductionTimeTicketEntries", x => new { x.Date, x.LineCode, x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.EmployeeCode, x.OperationCode });
                    table.ForeignKey(
                        name: "FK_DailyProductionTimeTicketEntries_Employees_EmployeeCode",
                        column: x => x.EmployeeCode,
                        principalTable: "Employees",
                        principalColumn: "EmployeeCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyProductionTimeTicketEntries_NonProductiveHourCodes_NonProductiveHourCode",
                        column: x => x.NonProductiveHourCode,
                        principalTable: "NonProductiveHourCodes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyProductionTimeTicketEntries_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyProductionTimeTicketEntries_ProductionLines_LineCode",
                        column: x => x.LineCode,
                        principalTable: "ProductionLines",
                        principalColumn: "LineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionTimeTicketEntries_EmployeeCode",
                table: "DailyProductionTimeTicketEntries",
                column: "EmployeeCode");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionTimeTicketEntries_LineCode",
                table: "DailyProductionTimeTicketEntries",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionTimeTicketEntries_NonProductiveHourCode",
                table: "DailyProductionTimeTicketEntries",
                column: "NonProductiveHourCode");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionTimeTicketEntries_OperationCode",
                table: "DailyProductionTimeTicketEntries",
                column: "OperationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProductionTimeTicketEntries");
        }
    }
}
