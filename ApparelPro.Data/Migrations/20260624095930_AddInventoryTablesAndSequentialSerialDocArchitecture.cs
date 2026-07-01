using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTablesAndSequentialSerialDocArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentCode);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSequences",
                columns: table => new
                {
                    NoteType = table.Column<string>(type: "varchar(5)", nullable: false),
                    LastAllocatedNumber = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "varchar(3)", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSequences", x => x.NoteType);
                });

            migrationBuilder.CreateTable(
                name: "OrderwiseStockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    TransactionType = table.Column<string>(type: "varchar(2)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    BuyerCode = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "varchar(12)", nullable: false),
                    DepartmentCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    StockCode = table.Column<string>(type: "varchar(2)", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(6)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(3)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    CreatedByUsername = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderwiseStockTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderwiseStockTransactions_DocumentNumber_TransactionType_StockCode_ItemCode",
                table: "OrderwiseStockTransactions",
                columns: new[] { "DocumentNumber", "TransactionType", "StockCode", "ItemCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "DocumentSequences");

            migrationBuilder.DropTable(
                name: "OrderwiseStockTransactions");
        }
    }
}
