using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateUnitConversionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitConversion",
                columns: table => new
                {
                    FromUnit = table.Column<string>(type: "varchar(3)", nullable: false),
                    ToUnit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Measure = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitConversion", x => new { x.FromUnit, x.ToUnit });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitConversion");
        }
    }
}
