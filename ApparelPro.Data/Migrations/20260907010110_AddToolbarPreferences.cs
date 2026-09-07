using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToolbarPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToolbarPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    ItemRouterLink = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolbarPins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolbarPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolbarPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolbarPins_UserEmail_GroupKey_ItemRouterLink",
                table: "ToolbarPins",
                columns: new[] { "UserEmail", "GroupKey", "ItemRouterLink" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolbarPreferences_UserEmail",
                table: "ToolbarPreferences",
                column: "UserEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolbarPins");

            migrationBuilder.DropTable(
                name: "ToolbarPreferences");
        }
    }
}
