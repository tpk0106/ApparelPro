using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class removequniqueconstraintfromstyletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Styles_Buyer_Order_Type_Style",
                table: "Styles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Styles_Buyer_Order_Type_Style",
                table: "Styles",
                columns: new[] { "Buyer", "Order", "Type", "Style" },
                unique: true);
        }
    }
}
