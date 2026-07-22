using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class CollapseStyleMaterialCostProfileItemCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear the table before reshaping it. Existing rows are only distinguished by the
            // StockCode/Feature1-4 columns being dropped below (e.g. Buyer=1, Order=1017-18,
            // Type=2, Style=ANCHORAGE, ItemCode=01FB covers 6 different fabric/color rows that
            // differ only by Feature1-4) — dropping those columns first would collapse several
            // existing rows onto the same (Buyer, Order, Type, Style, ItemCode) key and the
            // AddPrimaryKey below would fail with a duplicate-key error. Since this table is
            // being fully reseeded from StyelMaterialCostProfileFullData.rpt right after this
            // migration runs, there is no data here worth preserving through the reshape.
            migrationBuilder.Sql("DELETE FROM [StyleMaterialCostProfiles];");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StyleMaterialCostProfiles",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.DropColumn(
                name: "StockCode",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.DropColumn(
                name: "Feature1",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.DropColumn(
                name: "Feature2",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.DropColumn(
                name: "Feature3",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.DropColumn(
                name: "Feature4",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "StyleMaterialCostProfiles",
                type: "varchar(22)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(6)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StyleMaterialCostProfiles",
                table: "StyleMaterialCostProfiles",
                columns: new[] { "Buyer", "Order", "Type", "Style", "ItemCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StyleMaterialCostProfiles",
                table: "StyleMaterialCostProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "StyleMaterialCostProfiles",
                type: "varchar(6)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(22)");

            migrationBuilder.AddColumn<string>(
                name: "StockCode",
                table: "StyleMaterialCostProfiles",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feature1",
                table: "StyleMaterialCostProfiles",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feature2",
                table: "StyleMaterialCostProfiles",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feature3",
                table: "StyleMaterialCostProfiles",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feature4",
                table: "StyleMaterialCostProfiles",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StyleMaterialCostProfiles",
                table: "StyleMaterialCostProfiles",
                columns: new[] { "Buyer", "Order", "Type", "Style", "StockCode", "ItemCode", "Feature1", "Feature2", "Feature3", "Feature4" });
        }
    }
}
