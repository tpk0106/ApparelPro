using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStyleOperationBreakdownTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentOperationTemplates",
                columns: table => new
                {
                    ComponentCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    OperationSequence = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    MachineTypeCode = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    Sam = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NumberOfMachines = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentOperationTemplates", x => new { x.ComponentCode, x.OperationSequence });
                    table.ForeignKey(
                        name: "FK_ComponentOperationTemplates_GarmentComponents_ComponentCode",
                        column: x => x.ComponentCode,
                        principalTable: "GarmentComponents",
                        principalColumn: "ComponentCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentOperationTemplates_MachineTypes_MachineTypeCode",
                        column: x => x.MachineTypeCode,
                        principalTable: "MachineTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentOperationTemplates_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleComponentBreakdowns",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ComponentSequence = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleComponentBreakdowns", x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.ComponentSequence });
                    table.ForeignKey(
                        name: "FK_StyleComponentBreakdowns_GarmentComponents_ComponentCode",
                        column: x => x.ComponentCode,
                        principalTable: "GarmentComponents",
                        principalColumn: "ComponentCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleOperationBreakdowns",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ComponentSequence = table.Column<int>(type: "int", nullable: false),
                    OperationNumber = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    MachineTypeCode = table.Column<string>(type: "nvarchar(2)", nullable: false),
                    Sam = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Quota = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    NumberOfMachines = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleOperationBreakdowns", x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode, x.ComponentSequence, x.OperationNumber });
                    table.ForeignKey(
                        name: "FK_StyleOperationBreakdowns_GarmentComponents_ComponentCode",
                        column: x => x.ComponentCode,
                        principalTable: "GarmentComponents",
                        principalColumn: "ComponentCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleOperationBreakdowns_MachineTypes_MachineTypeCode",
                        column: x => x.MachineTypeCode,
                        principalTable: "MachineTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleOperationBreakdowns_Operations_OperationCode",
                        column: x => x.OperationCode,
                        principalTable: "Operations",
                        principalColumn: "OperationCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleProductionCapacities",
                columns: table => new
                {
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    TypeCode = table.Column<int>(type: "int", nullable: false),
                    StyleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    OutputPerDay = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleProductionCapacities", x => new { x.BuyerCode, x.Order, x.TypeCode, x.StyleCode });
                });

            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "ParameterKey", "Category", "DataType", "Description", "Options", "Value" },
                values: new object[,]
                {
                    { "ProductionDefaultMachineCountPerLine", "Production Control", "Number", "Legacy FACTPARA.NO_MCS - default assumed machine count used to size a style's target daily output before it's actually assigned to a specific Production Line (see ProductionLines.NumberOfMachines for the per-line figure used once a real line assignment exists).", null, "50" },
                    { "ProductionEfficiency1Percent", "Production Control", "Number", "Legacy FACTPARA.EFF1 - factory efficiency percentage. Not currently consumed by any migrated screen; carried over for the Reports phase.", null, "80" },
                    { "ProductionEfficiency2Percent", "Production Control", "Number", "Legacy FACTPARA.EFF2 - efficiency percentage applied to raw machine throughput in the Style Operation Breakdown line-balancing calculation.", null, "65" },
                    { "ProductionWorkHoursPerDay", "Production Control", "Number", "Standard shift length in hours, used to compute each operation's Quota (pieces/day at 100% = (WorkHoursPerDay*60)/SAM) and the line's target daily output in Style Operation Breakdown.", null, "8" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentOperationTemplates_MachineTypeCode",
                table: "ComponentOperationTemplates",
                column: "MachineTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentOperationTemplates_OperationCode",
                table: "ComponentOperationTemplates",
                column: "OperationCode");

            migrationBuilder.CreateIndex(
                name: "IX_StyleComponentBreakdowns_ComponentCode",
                table: "StyleComponentBreakdowns",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_StyleOperationBreakdowns_ComponentCode",
                table: "StyleOperationBreakdowns",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_StyleOperationBreakdowns_MachineTypeCode",
                table: "StyleOperationBreakdowns",
                column: "MachineTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_StyleOperationBreakdowns_OperationCode",
                table: "StyleOperationBreakdowns",
                column: "OperationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentOperationTemplates");

            migrationBuilder.DropTable(
                name: "StyleComponentBreakdowns");

            migrationBuilder.DropTable(
                name: "StyleOperationBreakdowns");

            migrationBuilder.DropTable(
                name: "StyleProductionCapacities");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "ProductionDefaultMachineCountPerLine");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "ProductionEfficiency1Percent");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "ProductionEfficiency2Percent");

            migrationBuilder.DeleteData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "ProductionWorkHoursPerDay");
        }
    }
}
