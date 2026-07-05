using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SrnConcurrencyAndSchemaFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStockTransactions",
                type: "varchar(22)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(6)");

            migrationBuilder.AddColumn<string>(
                name: "StoreCode",
                table: "OrderwiseStockTransactions",
                type: "varchar(3)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "OrderwiseStocks",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStocks",
                type: "varchar(22)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "OrderwiseStockMasters",
                type: "varchar(12)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStockMasters",
                type: "varchar(22)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStocks_BuyerCode_Order_StoreCode_ItemCode",
                table: "OrderwiseStocks",
                columns: new[] { "BuyerCode", "Order", "StoreCode", "ItemCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderwiseStocks_BuyerCode_Order_StoreCode_ItemCode",
                table: "OrderwiseStocks");

            migrationBuilder.DropColumn(
                name: "StoreCode",
                table: "OrderwiseStockTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStockTransactions",
                type: "varchar(6)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(22)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "OrderwiseStocks",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStocks",
                type: "varchar(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(22)");

            migrationBuilder.AlterColumn<string>(
                name: "Order",
                table: "OrderwiseStockMasters",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "OrderwiseStockMasters",
                type: "varchar(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(22)");
        }
    }
}
