using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReportRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportRegistries",
                columns: table => new
                {
                    ReportCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredParams = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParamSources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndpointTemplate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CommonNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRegistries", x => x.ReportCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRegistries_Category",
                table: "ReportRegistries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRegistries_IsActive",
                table: "ReportRegistries",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRegistries");
        }
    }
}
