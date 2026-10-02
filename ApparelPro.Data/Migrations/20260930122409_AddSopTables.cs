using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSopTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StandardOperatingProcedures",
                columns: table => new
                {
                    SopId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SopCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardOperatingProcedures", x => x.SopId);
                });

            migrationBuilder.CreateTable(
                name: "SopApplicabilities",
                columns: table => new
                {
                    SopApplicabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SopId = table.Column<int>(type: "int", nullable: false),
                    ApplicabilityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SopApplicabilities", x => x.SopApplicabilityId);
                    table.ForeignKey(
                        name: "FK_SopApplicabilities_StandardOperatingProcedures_SopId",
                        column: x => x.SopId,
                        principalTable: "StandardOperatingProcedures",
                        principalColumn: "SopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_ApplicabilityType_ApplicabilityKey",
                table: "SopApplicabilities",
                columns: new[] { "ApplicabilityType", "ApplicabilityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_SopApplicabilities_SopId_ApplicabilityType_ApplicabilityKey_IsExcluded",
                table: "SopApplicabilities",
                columns: new[] { "SopId", "ApplicabilityType", "ApplicabilityKey", "IsExcluded" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_Category",
                table: "StandardOperatingProcedures",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_IsActive",
                table: "StandardOperatingProcedures",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_IsActive_EffectiveFrom",
                table: "StandardOperatingProcedures",
                columns: new[] { "IsActive", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_StandardOperatingProcedures_SopCode",
                table: "StandardOperatingProcedures",
                column: "SopCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SopApplicabilities");

            migrationBuilder.DropTable(
                name: "StandardOperatingProcedures");
        }
    }
}
