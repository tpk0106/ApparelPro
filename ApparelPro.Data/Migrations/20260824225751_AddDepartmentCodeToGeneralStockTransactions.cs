using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentCodeToGeneralStockTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "GeneralStockTransactions",
                type: "varchar(3)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeneralStores",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralStores", x => x.Code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralStores");

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "GeneralStockTransactions");
        }
    }
}
