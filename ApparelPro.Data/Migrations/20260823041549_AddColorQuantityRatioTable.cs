using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColorQuantityRatioTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColorQuantityRatios",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    Color = table.Column<string>(type: "varchar(12)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: true),
                    Ratio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorQuantityRatios", x => new { x.Buyer, x.Order, x.Type, x.Style, x.Color });
                    table.ForeignKey(
                        name: "FK_ColorQuantityRatios_Styles_Buyer_Order_Type_Style",
                        columns: x => new { x.Buyer, x.Order, x.Type, x.Style },
                        principalTable: "Styles",
                        principalColumns: new[] { "Buyer", "Order", "Type", "Style" },
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorQuantityRatios");
        }
    }
}
