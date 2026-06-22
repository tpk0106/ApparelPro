using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMaterialConsumptionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemFeatures",
                columns: table => new
                {
                    FeatureCode = table.Column<string>(type: "varchar(4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemFeatures", x => x.FeatureCode);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemFeatures",
                columns: table => new
                {
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Feature1 = table.Column<string>(type: "varchar(4)", nullable: true),
                    Feature2 = table.Column<string>(type: "varchar(4)", nullable: true),
                    Feature3 = table.Column<string>(type: "varchar(4)", nullable: true),
                    Feature4 = table.Column<string>(type: "varchar(4)", nullable: true),
                    CostPerUnit = table.Column<decimal>(type: "decimal(10,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemFeatures", x => new { x.StockCode, x.ItemCode });
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => new { x.StockCode, x.ItemCode });
                });

            migrationBuilder.CreateTable(
                name: "StyleMaterialConsumptionLedger",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "varchar(20)", nullable: false),
                    Color = table.Column<string>(type: "varchar(6)", nullable: false),
                    Size = table.Column<string>(type: "varchar(10)", nullable: false),
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Feature1 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature2 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature3 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature4 = table.Column<string>(type: "varchar(4)", nullable: false),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ConsumptionUnit = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemUnit = table.Column<string>(type: "varchar(3)", nullable: false),
                    QuantityPerGarment = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    SupplierCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    TotalConsumption = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PercentageAllowance = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    IsAdditionalCost = table.Column<bool>(type: "bit", nullable: false),
                    CalculateConsumption = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleMaterialConsumptionLedger", x => new { x.Buyer, x.Order, x.Type, x.Style, x.Color, x.Size, x.StockCode, x.ItemCode, x.Feature1, x.Feature2, x.Feature3, x.Feature4 });
                });

            migrationBuilder.CreateTable(
                name: "StyleMaterialCostProfiles",
                columns: table => new
                {
                    Buyer = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Style = table.Column<string>(type: "varchar(20)", nullable: false),
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Feature1 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature2 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature3 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Feature4 = table.Column<string>(type: "varchar(4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(40)", nullable: false),
                    ItemUnit = table.Column<string>(type: "varchar(3)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    BalanceQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleMaterialCostProfiles", x => new { x.Buyer, x.Order, x.Type, x.Style, x.StockCode, x.ItemCode, x.Feature1, x.Feature2, x.Feature3, x.Feature4 });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemFeatures");

            migrationBuilder.DropTable(
                name: "OrderItemFeatures");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "StyleMaterialConsumptionLedger");

            migrationBuilder.DropTable(
                name: "StyleMaterialCostProfiles");
        }
    }
}
