using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToColorSizeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Colour Target Allocation Setup screen lets the user enter a free-text
            // description/shade name per colour, but this table had nowhere to persist
            // it - it was silently dropped on save. Nullable/denormalized per-row,
            // matching this table's existing flat design (Color is already repeated
            // per size row for the same colour).
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ColorSizeDetails",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ColorSizeDetails");
        }
    }
}
