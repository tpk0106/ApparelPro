using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateColorSizeDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColorSizeDetails",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "varchar(12)", nullable: false),
                    Color = table.Column<string>(type: "varchar(12)", nullable: false),
                    Size = table.Column<string>(type: "varchar(12)", nullable: false),
                    Ratio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorSizeDetails", x => new { x.Buyer, x.Order, x.Type, x.Style, x.Color, x.Size });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorSizeDetails");
        }
    }
}
