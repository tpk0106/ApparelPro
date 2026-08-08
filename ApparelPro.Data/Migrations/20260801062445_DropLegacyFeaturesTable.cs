using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    // Renamed from "AddBankCodeToAddress" - that name never matched what this migration
    // actually does (it just drops the unused legacy "Features" table). BankCode was
    // really added to Address back in 20260610054112_AddMultipleAddressesToBank.cs. The
    // migration ID (see the [Migration] attribute on the paired Designer.cs file) is left
    // UNCHANGED on purpose: it's already recorded in __EFMigrationsHistory on every
    // database this has run against, and changing it would make EF think this migration
    // was never applied and try to re-run DropTable("Features") against a database where
    // that table is already gone.
    public partial class DropLegacyFeaturesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Features");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });
        }
    }
}
