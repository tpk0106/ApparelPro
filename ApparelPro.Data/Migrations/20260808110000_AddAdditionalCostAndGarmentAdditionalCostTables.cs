using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalCostAndGarmentAdditionalCostTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalCosts",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalCosts", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "GarmentAdditionalCosts",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "varchar(20)", nullable: false),
                    AdditionalCostCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(22)", nullable: false),
                    Color = table.Column<string>(type: "varchar(6)", nullable: false),
                    Size = table.Column<string>(type: "varchar(10)", nullable: false),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    IsCostPerGarment = table.Column<bool>(type: "bit", nullable: false),
                    IsSemiFinishedGarment = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentAdditionalCosts", x => new { x.Buyer, x.Order, x.Type, x.Style, x.AdditionalCostCode, x.ItemCode });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentAdditionalCosts");

            migrationBuilder.DropTable(
                name: "AdditionalCosts");
        }
    }
}
