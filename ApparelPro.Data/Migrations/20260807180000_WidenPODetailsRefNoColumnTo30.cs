using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class WidenPODetailsRefNoColumnTo30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Follow-up to WidenPODetailsRefNoColumn (varchar(10) -> varchar(23)): the user asked
            // for extra headroom, so widened once more to varchar(30). The frontend "Reference
            // Number" field on the Supplier PO line-item form is capped at maxLength 30 to match.
            migrationBuilder.AlterColumn<string>(
                name: "RefNo",
                table: "PODetails",
                type: "varchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(23)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefNo",
                table: "PODetails",
                type: "varchar(23)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldNullable: true);
        }
    }
}
