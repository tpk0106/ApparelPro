using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyProductionEntriesAndContractSectionParam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyProductionEntries",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    LineCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    SectionCode = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(9,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProductionEntries", x => new { x.Date, x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.LineCode, x.SectionCode });
                    table.ForeignKey(
                        name: "FK_DailyProductionEntries_ProductionLines_LineCode",
                        column: x => x.LineCode,
                        principalTable: "ProductionLines",
                        principalColumn: "LineCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyProductionEntries_Sections_SectionCode",
                        column: x => x.SectionCode,
                        principalTable: "Sections",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "ParameterKey", "Category", "DataType", "Description", "Options", "Value" },
                values: new object[] { "ProductionContractSectionCode", "Production Control", "Text", "Legacy FACTPARA.CONTR_SECT - the Section (see Sections.Code) treated as the contractual production ceiling. No other section's running to-date quantity may exceed this section's running to-date quantity for the same style/line when saving Actual Production Entry. Defaults to 001 (Cutting).", null, "001" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionEntries_LineCode",
                table: "DailyProductionEntries",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionEntries_SectionCode",
                table: "DailyProductionEntries",
                column: "SectionCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProductionEntries");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "ProductionContractSectionCode");
        }
    }
}
