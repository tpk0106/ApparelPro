using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class WidenMaterialConsumptionDescriptionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Widen StyleMaterialCostProfiles.Description (the per-entry description shown on
            // the Supplier PO) from 40 to 100 chars -- 40 was too short for meaningful fabric/trim
            // descriptions like "PIQUE 58-60in Knit Fabric, White, Combed Cotton".
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "StyleMaterialCostProfiles",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)");

            // StockItems.Description is seeded from the same request.Description value the first
            // time a new StockCode/ItemCode combination is saved (see
            // MaterialConsumptionService.SaveMaterialConsumptionEntryAsync, Step 1) -- widened in
            // lockstep so a description longer than 50 chars doesn't throw a SQL truncation error
            // on insert.
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "StockItems",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "StyleMaterialCostProfiles",
                type: "varchar(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "StockItems",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");
        }
    }
}
