using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedProductionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstimatedProductionEntries",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(9,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimatedProductionEntries", x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.LineCode, x.Date });
                    table.ForeignKey(
                        name: "FK_EstimatedProductionEntries_ProductionLines_LineCode",
                        column: x => x.LineCode,
                        principalTable: "ProductionLines",
                        principalColumn: "LineCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstimatedProductionEntries_LineCode",
                table: "EstimatedProductionEntries",
                column: "LineCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstimatedProductionEntries");
        }
    }
}
