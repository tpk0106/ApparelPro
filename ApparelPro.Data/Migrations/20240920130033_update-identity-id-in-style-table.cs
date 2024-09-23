using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateidentityidinstyletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Styles",
            //    table: "Styles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Styles",
                table: "Styles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_Buyer_Order_Type_Style",
                table: "Styles",
                columns: new[] { "Buyer", "Order", "Type", "Style" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Styles",
                table: "Styles");

            migrationBuilder.DropIndex(
                name: "IX_Styles_Buyer_Order_Type_Style",
                table: "Styles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Styles",
                table: "Styles",
                columns: new[] { "Buyer", "Order", "Type", "Style" });
        }
    }
}
