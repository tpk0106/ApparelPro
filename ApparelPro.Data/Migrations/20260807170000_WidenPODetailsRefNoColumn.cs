using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class WidenPODetailsRefNoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PODetails.RefNo was varchar(10) - too narrow for real operator-entered reference
            // values (a Supplier PO save threw "String or binary data would be truncated" for a
            // 10+ char reference). Widened to varchar(23) to match LCNo, the other free-text
            // reference column on this same table.
            migrationBuilder.AlterColumn<string>(
                name: "RefNo",
                table: "PODetails",
                type: "varchar(23)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefNo",
                table: "PODetails",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(23)",
                oldNullable: true);
        }
    }
}
