using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralStockReferenceAndCostOfProductionReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralStockReferences",
                columns: table => new
                {
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(60)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralStockReferences", x => x.ItemCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralStockReferences");
        }
    }
}
