using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterPODetailsPONoToStringAndAddOrderwsieStockAndOrderwiseStockMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. OVERRIDE RE-DESTRUCT LOOP: Safely drop the old table and recreate it
            // with your exact composite keys, clearing out the identity/rename conflicts entirely.
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.PODetails;");

            migrationBuilder.Sql(@"
                CREATE TABLE dbo.PODetails (
                    PONumber VARCHAR(10) NOT NULL,
                    Buyer INT NOT NULL,
                    [Order] VARCHAR(12) NOT NULL,
                    Type INT NOT NULL,
                    Style VARCHAR(12) NOT NULL,
                    ItemCode VARCHAR(40) NOT NULL,
                    RefNo VARCHAR(10) NULL,
                    OrderUnit VARCHAR(3) NOT NULL,
                    OrderQuantity DECIMAL(12,2) NOT NULL,
                    UnitPrice DECIMAL(10,4) NOT NULL,
                    ExportDate DATETIME NULL,
                    LCNo VARCHAR(23) NULL,
                    Balance DECIMAL(12,2) NOT NULL,
                    CONSTRAINT PK_PODetails PRIMARY KEY CLUSTERED (PONumber, Buyer, [Order], Type, Style, ItemCode)
                );
            ");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_PODetails",
            //    table: "PODetails");

            //migrationBuilder.RenameColumn(
            //    name: "PONo",
            //    table: "PODetails",
            //    newName: "Id");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "UnitPrice",
            //    table: "PODetails",
            //    type: "decimal(10,4)",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(10,2)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "RefNo",
            //    table: "PODetails",
            //    type: "varchar(10)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(10)",
            //    oldMaxLength: 10);

            //migrationBuilder.AlterColumn<string>(
            //    name: "OrderUnit",
            //    table: "PODetails",
            //    type: "varchar(3)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(3)",
            //    oldMaxLength: 3);

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "OrderQuantity",
            //    table: "PODetails",
            //    type: "decimal(12,2)",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(10,2)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "LCNo",
            //    table: "PODetails",
            //    type: "varchar(23)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(23)",
            //    oldMaxLength: 23);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ItemCode",
            //    table: "PODetails",
            //    type: "varchar(40)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(22)",
            //    oldMaxLength: 22);

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "ExportDate",
            //    table: "PODetails",
            //    type: "datetime",
            //    nullable: true,
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "Balance",
            //    table: "PODetails",
            //    type: "decimal(12,2)",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Style",
            //    table: "PODetails",
            //    type: "varchar(12)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(12)",
            //    oldMaxLength: 12);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Order",
            //    table: "PODetails",
            //    type: "varchar(12)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(12)",
            //    oldMaxLength: 12);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Id",
            //    table: "PODetails",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .OldAnnotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AddColumn<string>(
            //    name: "PONumber",
            //    table: "PODetails",
            //    type: "varchar(10)",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_PODetails",
            //    table: "PODetails",
            //    columns: new[] { "PONumber", "Buyer", "Order", "Type", "Style", "ItemCode" });

            migrationBuilder.CreateTable(
                name: "OrderwiseStockMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(40)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderwiseStockMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderwiseStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(20)", nullable: false),
                    StoreCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(40)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderwiseStocks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderwiseStockMasters");

            migrationBuilder.DropTable(
                name: "OrderwiseStocks");
            

            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.PODetails;");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_PODetails",
            //    table: "PODetails");

            //migrationBuilder.DropColumn(
            //    name: "PONumber",
            //    table: "PODetails");

            //migrationBuilder.RenameColumn(
            //    name: "Id",
            //    table: "PODetails",
            //    newName: "PONo");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "UnitPrice",
            //    table: "PODetails",
            //    type: "decimal(10,2)",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(10,4)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "RefNo",
            //    table: "PODetails",
            //    type: "nvarchar(10)",
            //    maxLength: 10,
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "varchar(10)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "OrderUnit",
            //    table: "PODetails",
            //    type: "nvarchar(3)",
            //    maxLength: 3,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(3)");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "OrderQuantity",
            //    table: "PODetails",
            //    type: "decimal(10,2)",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(12,2)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "LCNo",
            //    table: "PODetails",
            //    type: "nvarchar(23)",
            //    maxLength: 23,
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "varchar(23)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "ExportDate",
            //    table: "PODetails",
            //    type: "datetime",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "Balance",
            //    table: "PODetails",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(12,2)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "ItemCode",
            //    table: "PODetails",
            //    type: "nvarchar(22)",
            //    maxLength: 22,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(40)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Style",
            //    table: "PODetails",
            //    type: "nvarchar(12)",
            //    maxLength: 12,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(12)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Order",
            //    table: "PODetails",
            //    type: "nvarchar(12)",
            //    maxLength: 12,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(12)");

            //migrationBuilder.AlterColumn<int>(
            //    name: "PONo",
            //    table: "PODetails",
            //    type: "int",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "int")
            //    .Annotation("SqlServer:Identity", "1, 1");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_PODetails",
            //    table: "PODetails",
            //    columns: new[] { "Buyer", "Order", "Type", "Style" });
        }
    }
}
