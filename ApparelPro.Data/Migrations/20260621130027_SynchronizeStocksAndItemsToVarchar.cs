using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizeStocksAndItemsToVarchar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FORCE INTERCEPT: Drop the dependent tables in correct dependency order to prevent constraint crashes
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.StockItems;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.Stocks;");

            // Rebuild Table 1: Master Stocks
                    migrationBuilder.Sql(@"
                CREATE TABLE dbo.Stocks (
                    StockCode VARCHAR(2) NOT NULL,
                    Description VARCHAR(30) NOT NULL,
                    CONSTRAINT PK_Stocks PRIMARY KEY CLUSTERED (StockCode)
                );
            ");

            // Rebuild Table 2: Child StockItems with a valid Foreign Key reference link
            migrationBuilder.Sql(@"
                CREATE TABLE dbo.StockItems (
                    StockCode VARCHAR(2) NOT NULL,
                    ItemCode VARCHAR(6) NOT NULL,
                    Description VARCHAR(50) NOT NULL,
                    CONSTRAINT PK_StockItems PRIMARY KEY CLUSTERED (StockCode, ItemCode),
                    CONSTRAINT FK_StockItems_Stocks_StockCode FOREIGN KEY (StockCode) REFERENCES dbo.Stocks (StockCode) ON DELETE NO ACTION 
                );
            ");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_StockItems_Stocks_StockId",
            //    table: "StockItems");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Stocks",
            //    table: "Stocks");

            //migrationBuilder.DropIndex(
            //    name: "IX_StockItems_StockId",
            //    table: "StockItems");

            //migrationBuilder.DropColumn(
            //    name: "Id",
            //    table: "Stocks");

            //migrationBuilder.DropColumn(
            //    name: "StockId",
            //    table: "StockItems");

            //migrationBuilder.DropColumn(
            //    name: "Id",
            //    table: "PODetails");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Description",
            //    table: "Stocks",
            //    type: "varchar(30)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(30)",
            //    oldMaxLength: 30);

            //migrationBuilder.AddColumn<string>(
            //    name: "StockCode",
            //    table: "Stocks",
            //    type: "varchar(2)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Description",
            //    table: "StockItems",
            //    type: "varchar(50)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(20)",
            //    oldMaxLength: 20);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ItemCode",
            //    table: "StockItems",
            //    type: "varchar(4)",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<string>(
            //    name: "StockCode",
            //    table: "StockItems",
            //    type: "varchar(2)",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_Stocks",
            //    table: "Stocks",
            //    column: "StockCode");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_StockItems_Stocks_StockCode",
            //    table: "StockItems",
            //    column: "StockCode",
            //    principalTable: "Stocks",
            //    principalColumn: "StockCode",
            //    onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.StockItems;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.Stocks;");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_StockItems_Stocks_StockCode",
            //    table: "StockItems");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Stocks",
            //    table: "Stocks");

            //migrationBuilder.DropColumn(
            //    name: "StockCode",
            //    table: "Stocks");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Description",
            //    table: "Stocks",
            //    type: "nvarchar(30)",
            //    maxLength: 30,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(30)");

            //migrationBuilder.AddColumn<int>(
            //    name: "Id",
            //    table: "Stocks",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0)
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Description",
            //    table: "StockItems",
            //    type: "nvarchar(20)",
            //    maxLength: 20,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(50)");

            //migrationBuilder.AlterColumn<int>(
            //    name: "ItemCode",
            //    table: "StockItems",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(4)")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AlterColumn<int>(
            //    name: "StockCode",
            //    table: "StockItems",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(2)");

            //migrationBuilder.AddColumn<int>(
            //    name: "StockId",
            //    table: "StockItems",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Id",
            //    table: "PODetails",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_Stocks",
            //    table: "Stocks",
            //    column: "Id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StockItems_StockId",
            //    table: "StockItems",
            //    column: "StockId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_StockItems_Stocks_StockId",
            //    table: "StockItems",
            //    column: "StockId",
            //    principalTable: "Stocks",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
