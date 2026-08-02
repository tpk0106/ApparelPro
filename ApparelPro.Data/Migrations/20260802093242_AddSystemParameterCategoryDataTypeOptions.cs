using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemParameterCategoryDataTypeOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SystemParameters",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "SystemParameters",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "SystemParameters",
                type: "varchar(1000)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "ParameterKey",
                keyValue: "AllowOrderQuantityOverride",
                columns: new[] { "Category", "DataType", "Options" },
                values: new object[] { "Order Management", "Boolean", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "SystemParameters");
        }
    }
}
