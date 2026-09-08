using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCompanyAddressSetupWithCompanyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyAddressSetups");

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
