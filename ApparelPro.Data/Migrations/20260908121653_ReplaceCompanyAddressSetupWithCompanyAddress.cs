using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Channels;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCompanyAddressSetupWithCompanyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //To your question — yes, it's perfectly safe to run that SQL in SSMS.
            //The __EFMigrationsHistory entry tells EF "this migration is already done, skip it."
            //If you ever need to reset the database and re-run all migrations from scratch
            //(like a fresh dotnet ef database update), EF will replay every migration in order
            //including the first one, and since a fresh DB won't have CompanyAddressSetups
            //either, it'll hit the same DROP TABLE error again.

            //To future-proof that, you can add a guard to the migration.
            //In your 20260908121653_ReplaceCompanyAddressSetupWithCompanyAddress.cs, change the Up method:

            // Guard: only drop if it exists (table may not exist on fresh DBs)
            migrationBuilder.Sql(
                "IF OBJECT_ID(N'[CompanyAddressSetups]', N'U') IS NOT NULL DROP TABLE [CompanyAddressSetups];");
            //migrationBuilder.DropTable(
            //    name: "CompanyAddressSetups");

            migrationBuilder.CreateTable(
                name: "CompanyAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressNo = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(40)", nullable: false),
                    Address1 = table.Column<string>(type: "varchar(40)", nullable: false),
                    Address2 = table.Column<string>(type: "varchar(40)", nullable: false),
                    City = table.Column<string>(type: "varchar(40)", nullable: false),
                    PostCode = table.Column<string>(type: "varchar(15)", nullable: false),
                    Country = table.Column<string>(type: "varchar(40)", nullable: false),
                    TelNos = table.Column<string>(type: "varchar(40)", nullable: false),
                    FaxNos = table.Column<string>(type: "varchar(40)", nullable: false),
                    TinNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    ExportRegNo = table.Column<string>(type: "varchar(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAddresses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyAddresses");

            migrationBuilder.CreateTable(
                name: "CompanyAddressSetups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address1 = table.Column<string>(type: "varchar(40)", nullable: false),
                    Address2 = table.Column<string>(type: "varchar(40)", nullable: false),
                    Address3 = table.Column<string>(type: "varchar(40)", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(40)", nullable: false),
                    ExportRegNo = table.Column<string>(type: "varchar(6)", nullable: false),
                    FaxNos = table.Column<string>(type: "varchar(40)", nullable: false),
                    TelNos = table.Column<string>(type: "varchar(40)", nullable: false),
                    TinNo = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAddressSetups", x => x.Id);
                });
        }
    }
}
